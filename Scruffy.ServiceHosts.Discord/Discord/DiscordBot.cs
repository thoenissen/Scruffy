using System.IO;
using System.Reflection;
using System.Text;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Exceptions;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.ServiceHosts.Discord.Discord;

/// <summary>
/// Management of the discord bot
/// </summary>
public sealed class DiscordBot : IAsyncDisposable
{
    #region Fields

    /// <summary>
    /// Service provider
    /// </summary>
    private ServiceProviderContainer _serviceProviderContainer;

    /// <summary>
    /// Is the task scheduler initialized?
    /// </summary>
    private bool _isSchedulerInitialized;

    /// <summary>
    /// Service scope
    /// </summary>
    private IServiceScope _serviceScope;

    /// <summary>
    /// Localization group
    /// </summary>
    private LocalizationGroup _localizationGroup;

    /// <summary>
    /// Client
    /// </summary>
    private DiscordSocketClient _discordClient;

    /// <summary>
    /// Interaction service
    /// </summary>
    private InteractionService _interaction;

    /// <summary>
    /// Debug channel id
    /// </summary>
    private ulong _debugChannel;

    /// <summary>
    /// Last disconnect exception
    /// </summary>
    private Exception _lastDisconnect;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public DiscordBot()
    {
        _serviceProviderContainer = new ServiceProviderContainer();
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Start the discord bot
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public async Task StartAsync()
    {
        var debugChannel = Environment.GetEnvironmentVariable("SCRUFFY_DEBUG_CHANNEL");
        if (string.IsNullOrWhiteSpace(debugChannel) == false)
        {
            _debugChannel = Convert.ToUInt64(debugChannel);
        }

        var config = new DiscordSocketConfig
                     {
                         LogLevel = LogSeverity.Info,
                         MessageCacheSize = 100,
                         GatewayIntents = GatewayIntents.Guilds
                                        | GatewayIntents.GuildMembers
                                        | GatewayIntents.GuildEmojis
                                        | GatewayIntents.GuildIntegrations
                                        | GatewayIntents.GuildVoiceStates
                                        | GatewayIntents.GuildPresences
                                        | GatewayIntents.GuildMessages
                                        | GatewayIntents.GuildMessageReactions
                                        | GatewayIntents.DirectMessages
                                        | GatewayIntents.DirectMessageReactions
                                        | GatewayIntents.MessageContent
                                        | GatewayIntents.GuildScheduledEvents
                     };

        _discordClient = new DiscordSocketClient(config);
        _discordClient.InteractionCreated += OnInteractionCreated;
        _discordClient.Log += OnDiscordClientLog;

        var interactionConfiguration = new InteractionServiceConfig
                                       {
                                           LogLevel = LogSeverity.Info,
                                           DefaultRunMode = RunMode.Async,
                                           ThrowOnError = true
                                       };

        _interaction = new InteractionService(_discordClient, interactionConfiguration);
        _interaction.Log += OnInteractionServiceLog;
        _interaction.ComponentCommandExecuted += OnComponentCommandExecuted;
        _interaction.SlashCommandExecuted += OnSlashCommandExecuted;
        _interaction.ModalCommandExecuted += OnModalCommandExecuted;

        _serviceProviderContainer.Initialize(obj =>
                                             {
                                                 obj.AddSingleton(_discordClient);
                                                 obj.AddSingleton<IDiscordClient>(_discordClient);
                                                 obj.AddSingleton(_interaction);
                                             });

        _serviceScope = _serviceProviderContainer.CreateScope();

        _localizationGroup = _serviceScope.ServiceProvider
                                          .GetRequiredService<LocalizationService>()
                                          .GetGroup(nameof(DiscordBot));

        _discordClient.Connected += OnConnected;
        _discordClient.Disconnected += OnDisconnected;

        await _interaction.AddModulesAsync(Assembly.Load("Scruffy.Commands"), _serviceScope.ServiceProvider)
                          .ConfigureAwait(false);

        await _discordClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_TOKEN"))
                            .ConfigureAwait(false);

        await _discordClient.StartAsync()
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// The discord client connected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    private async Task OnConnected()
    {
        if (_isSchedulerInitialized == false)
        {
            _isSchedulerInitialized = true;

            await _serviceScope.ServiceProvider
                               .GetRequiredService<JobScheduler>()
                               .StartAsync()
                               .ConfigureAwait(false);
        }

        if (_debugChannel > 0)
        {
            var channel = await _discordClient.GetChannelAsync(_debugChannel)
                                              .ConfigureAwait(false);

            if (channel is ITextChannel textChannel)
            {
                var message = new StringBuilder();

                message.AppendLine("The connection to Discord has been established.");

                message.AppendLine("```");

                message.AppendLine($"Version: {new FileInfo(Assembly.GetExecutingAssembly().Location).CreationTime:yyyy-MM-dd HH:mm:ss}");

                if (_lastDisconnect != null)
                {
                    message.Append("Last disconnect message: ");
                    message.AppendLine(_lastDisconnect.ToString());

                    _lastDisconnect = null;
                }

                message.AppendLine("```");

                await textChannel.SendMessageAsync(message.ToString())
                                 .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// The discord client disconnected
    /// </summary>
    /// <param name="ex">Exception</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    private Task OnDisconnected(Exception ex)
    {
        _lastDisconnect = ex;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Discord client logging
    /// </summary>
    /// <param name="e">Argument</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private Task OnDiscordClientLog(LogMessage e)
    {
        LoggingService.AddDiscordClientLog(e);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Interaction service logging
    /// </summary>
    /// <param name="e">Argument</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private Task OnInteractionServiceLog(LogMessage e)
    {
        LoggingService.AddInteractionServiceLog(e);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Interaction created
    /// </summary>
    /// <param name="e">Arguments</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnInteractionCreated(SocketInteraction e)
    {
        var context = new InteractionContextContainer(_discordClient, e);

        try
        {
            await _interaction.ExecuteCommandAsync(context, context.ServiceProvider)
                              .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await HandleInteractionException(context, ex).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handling command exceptions
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="ex">Exception</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task HandleInteractionException(InteractionContextContainer context, Exception ex)
    {
        if (ex is ScruffyException)
        {
            if (ex is ScruffyUserMessageException userException)
            {
                await context.SendMessageAsync($"{context.User.Mention} {userException.GetLocalizedMessage()}", ephemeral: true)
                             .ConfigureAwait(false);
            }
        }
        else
        {
            long? logEntryId;

            if (((IInteractionContext)context).Interaction?.Data is IComponentInteractionData interactionData)
            {
                logEntryId = LoggingService.AddInteractionLogEntry(LogEntryLevel.CriticalError, interactionData.CustomId, "Unhandled execution error", context.User.ToString(), ex);
            }
            else
            {
                logEntryId = LoggingService.AddInteractionLogEntry(LogEntryLevel.CriticalError, "unknown", "Unhandled execution error", context.User.ToString(), ex);
            }

            await context.SendMessageAsync(_localizationGroup.GetFormattedText("CommandFailedMessage", "The command could not be executed. (Error code 0x{0:X}).", logEntryId ?? -1),
                                           ephemeral: true)
                         .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Component command executed
    /// </summary>
    /// <param name="command">Command</param>
    /// <param name="context">Context</param>
    /// <param name="result">Result</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnComponentCommandExecuted(ComponentCommandInfo command, IInteractionContext context, IResult result)
    {
        if (context is InteractionContextContainer container)
        {
            if (((IInteractionContext)container).Interaction?.Data is IComponentInteractionData interactionData)
            {
                LoggingService.AddInteractionLogEntry(LogEntryLevel.Information, interactionData.CustomId, "Component command executed", container.User.ToString());
            }

            using (container)
            {
                if (result.IsSuccess == false)
                {
                    switch (result.Error)
                    {
                        case InteractionCommandError.Exception:
                            {
                                if (result is ExecuteResult executeResult)
                                {
                                    await HandleInteractionException(container, executeResult.Exception).ConfigureAwait(false);
                                }
                            }
                            break;
                        case InteractionCommandError.UnknownCommand:
                        case InteractionCommandError.ParseFailed:
                        case InteractionCommandError.ConvertFailed:
                        case InteractionCommandError.BadArgs:
                        case InteractionCommandError.UnmetPrecondition:
                        case InteractionCommandError.Unsuccessful:
                        case null:
                        default:
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Modal command executed
    /// </summary>
    /// <param name="command">Command</param>
    /// <param name="context">Context</param>
    /// <param name="result">Result</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnModalCommandExecuted(ModalCommandInfo command, IInteractionContext context, IResult result)
    {
        if (context is InteractionContextContainer container)
        {
            if (((IInteractionContext)container).Interaction?.Data is IComponentInteractionData interactionData)
            {
                LoggingService.AddInteractionLogEntry(LogEntryLevel.Information, interactionData.CustomId, "Modal executed", container.User.ToString());
            }

            using (container)
            {
                if (result.IsSuccess == false)
                {
                    switch (result.Error)
                    {
                        case InteractionCommandError.Exception:
                            {
                                if (result is ExecuteResult executeResult)
                                {
                                    await HandleInteractionException(container, executeResult.Exception).ConfigureAwait(false);
                                }
                            }
                            break;
                        case InteractionCommandError.UnknownCommand:
                        case InteractionCommandError.ParseFailed:
                        case InteractionCommandError.ConvertFailed:
                        case InteractionCommandError.BadArgs:
                        case InteractionCommandError.UnmetPrecondition:
                        case InteractionCommandError.Unsuccessful:
                        case null:
                        default:
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Slash command executed
    /// </summary>
    /// <param name="command">Command</param>
    /// <param name="context">Context</param>
    /// <param name="result">Result</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnSlashCommandExecuted(SlashCommandInfo command, IInteractionContext context, IResult result)
    {
        if (context is InteractionContextContainer container)
        {
            if (((IInteractionContext)container).Interaction?.Data is IComponentInteractionData interactionData)
            {
                LoggingService.AddInteractionLogEntry(LogEntryLevel.Information, interactionData.CustomId, "Slash command executed", container.User.ToString());
            }

            using (container)
            {
                if (result.IsSuccess == false)
                {
                    switch (result.Error)
                    {
                        case InteractionCommandError.Exception:
                            {
                                if (result is ExecuteResult executeResult)
                                {
                                    await HandleInteractionException(container, executeResult.Exception).ConfigureAwait(false);
                                }
                            }
                            break;
                        case InteractionCommandError.UnknownCommand:
                        case InteractionCommandError.ParseFailed:
                        case InteractionCommandError.ConvertFailed:
                        case InteractionCommandError.BadArgs:
                        case InteractionCommandError.UnmetPrecondition:
                        case InteractionCommandError.Unsuccessful:
                        case null:
                        default:
                            break;
                    }
                }
            }
        }
    }

    #endregion // Methods

    #region IAsyncDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_interaction != null)
        {
            _interaction.Dispose();
            _interaction = null;
        }

        if (_discordClient != null)
        {
            _discordClient.Connected -= OnConnected;
            _discordClient.Disconnected -= OnDisconnected;

            await _discordClient.LogoutAsync()
                                .ConfigureAwait(false);
            await _discordClient.DisposeAsync()
                                .ConfigureAwait(false);
            _discordClient = null;
        }

        if (_serviceScope != null)
        {
            _serviceScope.Dispose();
            _serviceScope = null;
        }

        if (_serviceProviderContainer != null)
        {
            await _serviceProviderContainer.DisposeAsync()
                                           .ConfigureAwait(false);
            _serviceProviderContainer = null;
        }
    }

    #endregion // IAsyncDisposable
}
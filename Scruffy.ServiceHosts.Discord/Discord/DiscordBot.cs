using System.Reflection;

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
        var config = new DiscordSocketConfig
                     {
                         LogLevel = LogSeverity.Info,
                         MessageCacheSize = 100,
                         GatewayIntents = GatewayIntents.Guilds
                                        | GatewayIntents.GuildMembers
                                        | GatewayIntents.GuildEmojis
                                        | GatewayIntents.GuildIntegrations
                                        | GatewayIntents.GuildVoiceStates
                                        | GatewayIntents.GuildMessages
                                        | GatewayIntents.GuildMessageReactions
                                        | GatewayIntents.DirectMessages
                                        | GatewayIntents.DirectMessageReactions
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
                                                 obj.AddSingleton(_interaction);
                                             });

        _serviceScope = _serviceProviderContainer.CreateScope();

        _localizationGroup = _serviceScope.ServiceProvider
                                          .GetRequiredService<LocalizationService>()
                                          .GetGroup(nameof(DiscordBot));

        _discordClient.Connected += OnConnected;

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
        _discordClient.Connected -= OnConnected;

        await _serviceScope.ServiceProvider
                           .GetRequiredService<JobScheduler>()
                           .StartAsync()
                           .ConfigureAwait(false);
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
using System.Net.Http;
using System.Reflection;

using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Json.Tenor;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Exceptions;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Extensions;

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
    /// Commands
    /// </summary>
    private CommandService _commands;

    /// <summary>
    /// Interaction service
    /// </summary>
    private InteractionService _interaction;

    /// <summary>
    /// Prefix resolver
    /// </summary>
    private PrefixResolvingService _prefixResolver;

    #endregion // Fields

    /// <summary>
    /// Constructor
    /// </summary>
    public DiscordBot()
    {
        _serviceProviderContainer = new ServiceProviderContainer();
    }

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
            GatewayIntents = GatewayIntents.All
        };

        _discordClient = new DiscordSocketClient(config);
        _discordClient.MessageReceived += OnMessageReceived;
        _discordClient.InteractionCreated += OnInteractionCreated;
        _discordClient.Log += OnDiscordClientLog;

        var commandConfiguration = new CommandServiceConfig
        {
            LogLevel = LogSeverity.Info,
            CaseSensitiveCommands = false,
            DefaultRunMode = global::Discord.Commands.RunMode.Async,
            IgnoreExtraArgs = false,
            ThrowOnError = true
        };

        _commands = new CommandService(commandConfiguration);
        _commands.CommandExecuted += OnCommandExecuted;
        _commands.Log += OnCommandServiceLog;

        var interactionConfiguration = new InteractionServiceConfig
        {
            LogLevel = LogSeverity.Info,
            DefaultRunMode = global::Discord.Interactions.RunMode.Async,
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
                                                 obj.AddSingleton(_commands);
                                                 obj.AddSingleton(_interaction);
                                             });

        _serviceScope = _serviceProviderContainer.CreateScope();

        _localizationGroup = _serviceScope.ServiceProvider
                                          .GetRequiredService<LocalizationService>()
                                          .GetGroup(nameof(DiscordBot));

        _prefixResolver = _serviceScope.ServiceProvider
                                       .GetRequiredService<PrefixResolvingService>();

        _discordClient.Connected += OnConnected;

        await _interaction.AddModulesAsync(Assembly.Load("Scruffy.Commands"), _serviceScope.ServiceProvider)
                          .ConfigureAwait(false);

        await _commands.AddModulesAsync(Assembly.Load("Scruffy.Commands"), _serviceScope.ServiceProvider)
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
    /// Commands service logging
    /// </summary>
    /// <param name="e">Argument</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private Task OnCommandServiceLog(LogMessage e)
    {
        LoggingService.AddCommandServiceLog(e);

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
    /// Fired when a message is received.
    /// </summary>
    /// <param name="e">Message</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnMessageReceived(SocketMessage e)
    {
        if (e is SocketUserMessage msg)
        {
            if (msg.Author.Id != _discordClient.CurrentUser.Id
             && msg.Author.IsBot == false)
            {
                var pos = 0;
                if (msg.HasStringPrefix(_prefixResolver.GetPrefix(msg), ref pos, StringComparison.InvariantCultureIgnoreCase)
                 || msg.HasMentionPrefix(_discordClient.CurrentUser, ref pos))
                {
                    var context = new CommandContextContainer(_discordClient, msg);

                    try
                    {
                        await _commands.ExecuteAsync(context, pos, context.ServiceProvider)
                                       .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        await HandleCommandException(context, ex).ConfigureAwait(false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Command executed
    /// </summary>
    /// <param name="commandInfo">Command info</param>
    /// <param name="context">Context</param>
    /// <param name="result">Result</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnCommandExecuted(Optional<CommandInfo> commandInfo, ICommandContext context, global::Discord.Commands.IResult result)
    {
        if (context is CommandContextContainer container)
        {
            using (container)
            {
                if (result.IsSuccess == false)
                {
                    switch (result.Error)
                    {
                        case CommandError.UnknownCommand:
                        case CommandError.ParseFailed:
                        case CommandError.BadArgCount:
                        case CommandError.ObjectNotFound:
                        case CommandError.MultipleMatches:
                            {
                                var pos = 0;

                                if (container.Message.HasStringPrefix(_prefixResolver.GetPrefix(container.Message), ref pos, StringComparison.InvariantCultureIgnoreCase)
                                 || container.Message.HasMentionPrefix(_discordClient.CurrentUser, ref pos))
                                {
                                    await container.Operations
                                                   .ShowHelp(container.Message.Content[pos..])
                                                   .ConfigureAwait(false);
                                }
                            }

                            break;

                        case CommandError.Exception:
                            {
                                if (result is global::Discord.Commands.ExecuteResult executeResult)
                                {
                                    await HandleCommandException(container, executeResult.Exception).ConfigureAwait(false);
                                }
                            }
                            break;

                        case CommandError.UnmetPrecondition:
                            {
                                await container.Operations
                                               .ShowUnmetPrecondition()
                                               .ConfigureAwait(false);
                            }
                            break;
                        case CommandError.Unsuccessful:
                        default:
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handling command exceptions
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="ex">Exception</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task HandleCommandException(CommandContextContainer context, Exception ex)
    {
        if (ex is ScruffyException)
        {
            if (ex is ScruffyUserMessageException userException)
            {
                await context.Channel
                             .SendMessageAsync($"{context.Message.Author.Mention} {userException.GetLocalizedMessage()}")
                             .ConfigureAwait(false);
            }
        }
        else
        {
            var logEntryId = LoggingService.AddTextCommandLogEntry(LogEntryLevel.CriticalError, context.Command?.GetFullName(), null, ex.Message, ex.ToString());

            var client = context.ServiceProvider.GetService<IHttpClientFactory>().CreateClient();

            using (var response = await client.GetAsync("https://g.tenor.com/v1/search?q=funny%20cat&key=RXM3VE2UGRU9&limit=50&contentfilter=high&ar_range=all")
                                              .ConfigureAwait(false))
            {
                var jsonResult = await response.Content
                                               .ReadAsStringAsync()
                                               .ConfigureAwait(false);

                var searchResult = JsonConvert.DeserializeObject<SearchResultRoot>(jsonResult);
                if (searchResult != null)
                {
                    var tenorEntry = searchResult.Results[new Random(DateTime.Now.Millisecond).Next(0, searchResult.Results.Count - 1)];

                    var gifUrl = tenorEntry.Media[0].Gif.Size < 8_388_608
                                     ? tenorEntry.Media[0].Gif.Url
                                     : tenorEntry.Media[0].MediumGif.Size < 8_388_608
                                         ? tenorEntry.Media[0].MediumGif.Url
                                         : tenorEntry.Media[0].NanoGif.Url;

                    using (var downloadResponse = await client.GetAsync(gifUrl)
                                                              .ConfigureAwait(false))
                    {
                        var stream = await downloadResponse.Content
                                                           .ReadAsStreamAsync()
                                                           .ConfigureAwait(false);

                        await using (stream.ConfigureAwait(false))
                        {
                            await context.Channel
                                         .SendFilesAsync(new[] { new FileAttachment(stream, "cat.gif") },
                                                         _localizationGroup.GetFormattedText("CommandFailedMessage", "The command could not be executed. But I have an error code ({0}) and funny cat picture.", logEntryId ?? -1))
                                         .ConfigureAwait(false);
                        }
                    }
                }
            }
        }
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
            await HandleCommandException(context, ex).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handling command exceptions
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="ex">Exception</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task HandleCommandException(InteractionContextContainer context, Exception ex)
    {
        if (ex is ScruffyException)
        {
            if (ex is ScruffyUserMessageException userException)
            {
                await context.Channel
                             .SendMessageAsync($"{context.User.Mention} {userException.GetLocalizedMessage()}")
                             .ConfigureAwait(false);
            }
        }
        else
        {
            if (((IInteractionContext)context).Interaction.Data is IComponentInteractionData interactionData)
            {
                var logEntryId = LoggingService.AddInteractionLogEntry(LogEntryLevel.CriticalError, interactionData.CustomId, ex);

                var client = context.ServiceProvider.GetService<IHttpClientFactory>().CreateClient();

                using (var response = await client.GetAsync("https://g.tenor.com/v1/search?q=funny%20cat&key=RXM3VE2UGRU9&limit=50&contentfilter=high&ar_range=all")
                                                  .ConfigureAwait(false))
                {
                    var jsonResult = await response.Content
                                                   .ReadAsStringAsync()
                                                   .ConfigureAwait(false);

                    var searchResult = JsonConvert.DeserializeObject<SearchResultRoot>(jsonResult);
                    if (searchResult != null)
                    {
                        var tenorEntry = searchResult.Results[new Random(DateTime.Now.Millisecond).Next(0, searchResult.Results.Count - 1)];

                        var gifUrl = tenorEntry.Media[0].Gif.Size < 8_388_608
                                         ? tenorEntry.Media[0].Gif.Url
                                         : tenorEntry.Media[0].MediumGif.Size < 8_388_608
                                             ? tenorEntry.Media[0].MediumGif.Url
                                             : tenorEntry.Media[0].NanoGif.Url;

                        using (var downloadResponse = await client.GetAsync(gifUrl)
                                                                  .ConfigureAwait(false))
                        {
                            var stream = await downloadResponse.Content
                                                               .ReadAsStreamAsync()
                                                               .ConfigureAwait(false);

                            await using (stream.ConfigureAwait(false))
                            {
                                await context.Channel
                                             .SendFilesAsync(new[] { new FileAttachment(stream, "cat.gif") },
                                                             _localizationGroup.GetFormattedText("CommandFailedMessage", "The command could not be executed. But I have an error code ({0}) and funny cat picture.", logEntryId ?? -1))
                                             .ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Component command executed
    /// </summary>
    /// <param name="command">Command</param>
    /// <param name="context">Context</param>
    /// <param name="result">Result</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnComponentCommandExecuted(ComponentCommandInfo command, IInteractionContext context, global::Discord.Interactions.IResult result)
    {
        if (context is InteractionContextContainer container)
        {
            using (container)
            {
                if (result.IsSuccess == false)
                {
                    switch (result.Error)
                    {
                        case InteractionCommandError.Exception:
                            {
                                if (result is global::Discord.Interactions.ExecuteResult executeResult)
                                {
                                    await HandleCommandException(container, executeResult.Exception).ConfigureAwait(false);
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
    private async Task OnModalCommandExecuted(ModalCommandInfo command, IInteractionContext context, global::Discord.Interactions.IResult result)
    {
        if (context is InteractionContextContainer container)
        {
            using (container)
            {
                if (result.IsSuccess == false)
                {
                    switch (result.Error)
                    {
                        case InteractionCommandError.Exception:
                            {
                                if (result is global::Discord.Interactions.ExecuteResult executeResult)
                                {
                                    await HandleCommandException(container, executeResult.Exception).ConfigureAwait(false);
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
    private async Task OnSlashCommandExecuted(SlashCommandInfo command, IInteractionContext context, global::Discord.Interactions.IResult result)
    {
        if (context is InteractionContextContainer container)
        {
            using (container)
            {
                if (result.IsSuccess == false)
                {
                    switch (result.Error)
                    {
                        case InteractionCommandError.Exception:
                            {
                                if (result is global::Discord.Interactions.ExecuteResult executeResult)
                                {
                                    await HandleCommandException(container, executeResult.Exception).ConfigureAwait(false);
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
        if (_commands != null)
        {
            ((IDisposable)_commands).Dispose();
            _commands = null;
        }

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

            _discordClient.Dispose();
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
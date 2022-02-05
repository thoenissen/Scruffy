using System.Net.Http;
using System.Reflection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Json.Tenor;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Exceptions;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Extensions;

namespace Scruffy.ServiceHost.Discord;

/// <summary>
/// Management of the discord bot
/// </summary>
public sealed class DiscordBot : IAsyncDisposable
{
    #region Fields

    /// <summary>
    /// Localization service
    /// </summary>
    private LocalizationService _localizationService;

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
    /// Prefix resolver
    /// </summary>
    private PrefixResolvingService _prefixResolver;

    /// <summary>
    /// Administration permissions validation
    /// </summary>
    private AdministrationPermissionsValidationService _administrationPermissionsValidationService;

    /// <summary>
    /// Blocked channel service
    /// </summary>
    private BlockedChannelService _blockedChannelService;

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Start the discord bot
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public async Task StartAsync(LocalizationService localizationService)
    {
        _localizationService = localizationService;
        _localizationGroup = _localizationService.GetGroup(nameof(DiscordBot));

        var config = new DiscordSocketConfig
                     {
                         LogLevel = LogSeverity.Info,
                         MessageCacheSize = 100
                     };

        _discordClient = new DiscordSocketClient(config);
        _discordClient.MessageReceived += OnMessageReceived;

        _prefixResolver = new PrefixResolvingService();
        _administrationPermissionsValidationService = new AdministrationPermissionsValidationService();
        _blockedChannelService = new BlockedChannelService(localizationService);

        var commandConfiguration = new CommandServiceConfig
                                   {
                                       LogLevel = LogSeverity.Info,
                                       CaseSensitiveCommands = false,
                                       DefaultRunMode = RunMode.Async,
                                       IgnoreExtraArgs = false,
                                       ThrowOnError = true
                                   };

        _commands = new CommandService(commandConfiguration);
        _commands.CommandExecuted += OnCommandExecuted;

        GlobalServiceProvider.Current.AddSingleton(_discordClient);
        GlobalServiceProvider.Current.AddSingleton(_prefixResolver);
        GlobalServiceProvider.Current.AddSingleton(_administrationPermissionsValidationService);
        GlobalServiceProvider.Current.AddSingleton(_blockedChannelService);
        GlobalServiceProvider.Current.AddSingleton(new DiscordStatusService(_discordClient));
        GlobalServiceProvider.Current.AddSingleton(new InteractionService(_discordClient));
        GlobalServiceProvider.Current.AddSingleton(_commands);

        await _commands.AddModulesAsync(Assembly.Load("Scruffy.Commands"), GlobalServiceProvider.Current.GetServiceProvider())
                       .ConfigureAwait(false);

        await _discordClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_TOKEN"))
                            .ConfigureAwait(false);

        await _discordClient.StartAsync()
                            .ConfigureAwait(false);
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
    private async Task OnCommandExecuted(Optional<CommandInfo> commandInfo, ICommandContext context, IResult result)
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
                                if (result is ExecuteResult executeResult)
                                {
                                    await HandleCommandException(container, executeResult.Exception).ConfigureAwait(false);
                                }
                            }
                            break;

                        case CommandError.UnmetPrecondition:
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
        if (ex is ScruffyException scruffyException)
        {
            await context.Channel
                         .SendMessageAsync($"{context.Message.Author.Mention} {scruffyException.GetLocalizedMessage()}")
                         .ConfigureAwait(false);
        }
        else
        {
            var logEntryId = LoggingService.AddCommandLogEntry(LogEntryLevel.CriticalError, context.Command?.GetFullName(), null, ex.Message, ex.ToString());

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

        if (_discordClient != null)
        {
            await _discordClient.LogoutAsync()
                                .ConfigureAwait(false);

            _discordClient.Dispose();
            _discordClient = null;
        }
    }

    #endregion // IAsyncDisposable
}
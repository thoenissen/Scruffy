using System.Net.Http;

using Newtonsoft.Json;

using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Json.Tenor;
using Scruffy.Services.Core.Exceptions;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;

namespace Scruffy.Services.Core.Discord;

/// <summary>
/// Command module base class with localization services
/// </summary>
public class LocatedCommandModuleBase : BaseCommandModule
{
    #region Fields

    /// <summary>
    /// Localization group
    /// </summary>
    private LocalizationGroup _localizationGroup;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Localization service
    /// </summary>
    public LocalizationService LocalizationService { protected get; set; }

    /// <summary>
    /// User management service
    /// </summary>
    public UserManagementService UserManagementService { protected get; set; }

    /// <summary>
    /// HttpClient-Factory
    /// </summary>
    public IHttpClientFactory HttpClientFactory { protected get; set; }

    /// <summary>
    /// Localization group
    /// </summary>
    public LocalizationGroup LocalizationGroup => _localizationGroup ??= LocalizationService.GetGroup(GetType().Name);

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Called before a command in the implementing module is executed.
    /// </summary>
    /// <param name="commandContext">Context in which the method is being executed.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override Task BeforeExecutionAsync(CommandContext commandContext)
    {
        LoggingService.AddCommandLogEntry(LogEntryLevel.Information, commandContext.Command?.QualifiedName, null, "BeforeExecution");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Execution
    /// </summary>
    /// <param name="commandContext">Original command context</param>
    /// <param name="action">Action</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected async Task InvokeAsync(CommandContext commandContext, Func<CommandContextContainer, Task> action)
    {
        var commandContextContainer = new CommandContextContainer(commandContext, UserManagementService);

        try
        {
            await action(commandContextContainer).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
        }
        catch (OperationCanceledException)
        {
        }
        catch (ScruffyException ex)
        {
            await commandContextContainer.Channel
                                         .SendMessageAsync(ex.GetLocalizedMessage())
                                         .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var logEntryId = LoggingService.AddCommandLogEntry(LogEntryLevel.CriticalError, commandContext.Command?.QualifiedName, commandContextContainer.LastUserMessage?.Content, ex.Message, ex.ToString());

            var client = HttpClientFactory.CreateClient();

            using (var response = await client.GetAsync("https://g.tenor.com/v1/search?q=funny%20cat&key=RXM3VE2UGRU9&limit=50&contentfilter=high&ar_range=all")
                                              .ConfigureAwait(false))
            {
                var jsonResult = await response.Content
                                               .ReadAsStringAsync()
                                               .ConfigureAwait(false);

                var searchResult = JsonConvert.DeserializeObject<SearchResultRoot>(jsonResult);

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
                        var internalLocalizationGroup = LocalizationService.GetGroup(nameof(LocatedCommandModuleBase));

                        var builder = new DiscordMessageBuilder().WithContent(internalLocalizationGroup.GetFormattedText("CommandFailedMessage", "The command could not be executed. But I have an error code ({0}) and funny cat picture.", logEntryId ?? -1))
                                                                 .WithFile("cat.gif", stream);

                        await commandContextContainer.Channel
                                                     .SendMessageAsync(builder)
                                                     .ConfigureAwait(false);
                    }
                }
            }
        }
    }

    #endregion // Methods
}
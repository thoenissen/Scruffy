using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

using Newtonsoft.Json;

using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Json.Tenor;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Commands
{
    /// <summary>
    /// Command module base class with localization services
    /// </summary>
    public class LocatedCommandModuleBase : BaseCommandModule
    {
        #region Fields

        /// <summary>
        /// Internal localization group
        /// </summary>
        private Lazy<LocalizationGroup> _internalLocalizationGroup;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public LocatedCommandModuleBase(LocalizationService localizationService)
        {
            LocalizationGroup = localizationService.GetGroup(GetType().Name);
            _internalLocalizationGroup = new Lazy<LocalizationGroup>(() => localizationService.GetGroup(nameof(LocatedCommandModuleBase)));
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Localization group
        /// </summary>
        public LocalizationGroup LocalizationGroup { get; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Execution
        /// </summary>
        /// <param name="commandContext">Original command context</param>
        /// <param name="action">Action</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task InvokeAsync(CommandContext commandContext, Func<CommandContextContainer, Task> action)
        {
            var commandContextContainer = new CommandContextContainer(commandContext);

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
            catch (Exception ex)
            {
                var logEntryId = LoggingService.AddCommandLogEntry(LogEntryLevel.CriticalError, commandContext.Command?.QualifiedName, commandContextContainer.LastUserMessage?.Content, ex.Message, ex.ToString());

                using (var response = await WebRequest.CreateHttp("https://g.tenor.com/v1/search?q=funny%20cat&key=RXM3VE2UGRU9&limit=50&contentfilter=high&ar_range=all")
                                                      .GetResponseAsync()
                                                      .ConfigureAwait(false))
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var jsonResult = await reader.ReadToEndAsync()
                                                     .ConfigureAwait(false);

                        var searchResult = JsonConvert.DeserializeObject<SearchResultRoot>(jsonResult);

                        using (var webClient = new WebClient())
                        {
                            var tenorEntry = searchResult.Results[new Random(DateTime.Now.Millisecond).Next(0, searchResult.Results.Count - 1)];

                            string gifUrl;

                            if (tenorEntry.Media[0]
                                          .Gif.Size
                              < 8_388_608)
                            {
                                gifUrl = tenorEntry.Media[0]
                                                   .Gif.Url;
                            }
                            else if (tenorEntry.Media[0]
                                               .MediumGif.Size
                                   < 8_388_608)
                            {
                                gifUrl = tenorEntry.Media[0]
                                                   .MediumGif.Url;
                            }
                            else
                            {
                                gifUrl = tenorEntry.Media[0]
                                                   .NanoGif.Url;
                            }

                            await using (var stream = new MemoryStream(webClient.DownloadData(gifUrl)))
                            {
                                var builder = new DiscordMessageBuilder().WithContent(_internalLocalizationGroup.Value.GetFormattedText("CommandFailedMessage", "The command could not be executed. But I have an error code ({0}) and funny cat picture.", logEntryId ?? -1))
                                                                         .WithFile("cat.gif", stream);

                                await commandContextContainer.Channel
                                                             .SendMessageAsync(builder)
                                                             .ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
        }

        #endregion // Methods
    }
}

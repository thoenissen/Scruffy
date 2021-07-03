using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Google.Apis.Customsearch.v1;
using Google.Apis.Services;

using Scruffy.Commands.Base;
using Scruffy.Services.Core;

namespace Scruffy.Commands
{
    /// <summary>
    /// Searching the web
    /// </summary>
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class SearchCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public SearchCommandModule(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Command methods

        /// <summary>
        /// Searching google
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <param name="searchTerm">Search term</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Command("google")]
        public async Task Google(CommandContext commandContext, [RemainingText] string searchTerm)
        {
            using (var customSearchService = new CustomsearchService(new BaseClientService.Initializer
                                                                     {
                                                                         ApiKey = Environment.GetEnvironmentVariable("SCRUFFY_GOOGLE_API_KEY")
                                                                     }))
            {
                var request = customSearchService.Cse.List();

                request.C2coff = "1";
                request.Cx = Environment.GetEnvironmentVariable("SCRUFFY_GOOGLE_CSE_ID");
                request.Num = 6;
                request.OrTerms = searchTerm;
                request.Safe = CseResource.ListRequest.SafeEnum.Active;

                var resultContainer = await request.ExecuteAsync()
                                          .ConfigureAwait(false);

                if (resultContainer.Items?.Count > 0)
                {
                    var embedBuilder = new DiscordEmbedBuilder
                                       {
                                           Color = DiscordColor.Green
                                       };

                    embedBuilder.WithTitle(LocalizationGroup.GetText("SearchResults", "Search results"));

                    foreach (var result in resultContainer.Items.Take(6))
                    {
                        embedBuilder.AddField(result.Title, Formatter.MaskedUrl(result.Snippet, new Uri(result.Link)));
                    }

                    await commandContext.RespondAsync(embedBuilder)
                                        .ConfigureAwait(false);
                }
                else
                {
                    await commandContext.RespondAsync(LocalizationGroup.GetText("NoResults", "I couldn't find anything."))
                                        .ConfigureAwait(false);
                }
            }
        }

        #endregion // Command methods
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.AspNetCore.WebUtilities;

using Newtonsoft.Json;

using Scruffy.Data.Json.Tenor;
using Scruffy.Services.Core;

namespace Scruffy.Commands
{
    /// <summary>
    /// GIF commands
    /// </summary>
    [Group("gif")]
    [Aliases("gi")]
    public class GifCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public GifCommandModule(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// GIF related to a string
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <param name="searchTerm">Search term</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [GroupCommand]
        public Task GroupCommand(CommandContext commandContext, [RemainingText]string searchTerm)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   if (string.IsNullOrWhiteSpace(searchTerm) == false)
                                   {
                                       await commandContext.Message
                                                           .DeleteAsync()
                                                           .ConfigureAwait(false);

                                       var rnd = new Random(DateTime.Now.Millisecond);

                                       using (var request = await WebRequest.CreateHttp(QueryHelpers.AddQueryString("https://g.tenor.com/v1/search",
                                                                                                         new Dictionary<string, string>
                                                                                                         {
                                                                                                             ["q"] = searchTerm,
                                                                                                             ["key"] = "RXM3VE2UGRU9",
                                                                                                             ["limit"] = "10",
                                                                                                             ["contentfilter"] = "high",
                                                                                                             ["ar_range"] = "all",
                                                                                                             ["media_filter"] = "minimal"
                                                                                                         }))
                                                                 .GetResponseAsync()
                                                                 .ConfigureAwait(false))
                                       {
                                           using (var reader = new StreamReader(request.GetResponseStream()))
                                           {
                                               var jsonResult = await reader.ReadToEndAsync()
                                                                            .ConfigureAwait(false);

                                               var searchResult = JsonConvert.DeserializeObject<SearchResultRoot>(jsonResult);

                                               await commandContext.Channel
                                                                   .SendMessageAsync(searchResult.Results[rnd.Next(searchResult.Results.Count - 1)].ItemUrl)
                                                                   .ConfigureAwait(false);
                                           }
                                       }
                                   }
                                   else
                                   {
                                       await commandContextContainer.ShowHelp("gif")
                                                                    .ConfigureAwait(false);
                                   }
                               });
        }

        #endregion // Methods
    }
}
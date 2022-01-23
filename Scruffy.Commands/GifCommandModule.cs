using System.Net.Http;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.AspNetCore.WebUtilities;

using Newtonsoft.Json;

using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Json.Tenor;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Commands;

/// <summary>
/// GIF commands
/// </summary>
[Group("gif")]
[Aliases("gi")]
[BlockedChannelCheck]
public class GifCommandModule : LocatedCommandModuleBase
{
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

                                   var client = HttpClientFactory.CreateClient();

                                   using (var request = await client.GetAsync(QueryHelpers.AddQueryString("https://g.tenor.com/v1/search",
                                                                                                          new Dictionary<string, string>
                                                                                                          {
                                                                                                              ["q"] = searchTerm,
                                                                                                              ["key"] = "RXM3VE2UGRU9",
                                                                                                              ["limit"] = "10",
                                                                                                              ["contentfilter"] = "high",
                                                                                                              ["ar_range"] = "all",
                                                                                                              ["media_filter"] = "minimal"
                                                                                                          }))
                                                                    .ConfigureAwait(false))
                                   {
                                       var jsonResult = await request.Content
                                                                     .ReadAsStringAsync()
                                                                     .ConfigureAwait(false);

                                       var searchResult = JsonConvert.DeserializeObject<SearchResultRoot>(jsonResult);

                                       await commandContext.Channel
                                                           .SendMessageAsync(searchResult.Results[rnd.Next(searchResult.Results.Count - 1)]
                                                                                         .ItemUrl)
                                                           .ConfigureAwait(false);

                                       LoggingService.AddCommandLogEntry(LogEntryLevel.Information, commandContext.Command.QualifiedName, searchTerm, commandContext.User.ToString());
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
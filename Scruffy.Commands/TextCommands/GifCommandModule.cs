using System.Net.Http;

using Discord.Commands;

using Microsoft.AspNetCore.WebUtilities;

using Newtonsoft.Json;

using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Json.Tenor;
using Scruffy.Services.Core;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;
using Scruffy.Services.Discord.Extensions;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// GIF commands
/// </summary>
[Group("gif")]
[Alias("gi")]
[BlockedChannelCheck]
public class GifCommandModule : LocatedTextCommandModuleBase
{
    #region Methods

    /// <summary>
    /// GIF related to a string
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command]
    public async Task GroupCommand([Remainder] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) == false)
        {
            await Context.Message
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
                if (searchResult != null)
                {
                    await Context.Channel
                                 .SendMessageAsync(searchResult.Results[rnd.Next(searchResult.Results.Count - 1)].ItemUrl)
                                 .ConfigureAwait(false);

                    LoggingService.AddTextCommandLogEntry(LogEntryLevel.Information, Context.Command.GetFullName(), searchTerm, Context.User.ToString());
                }
            }
        }
        else
        {
            await Context.Operations
                         .ShowHelp("gif")
                         .ConfigureAwait(false);
        }
    }

    #endregion // Methods
}
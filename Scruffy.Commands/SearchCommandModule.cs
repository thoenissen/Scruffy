using System.Net.Http;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Google.Apis.Customsearch.v1;
using Google.Apis.Services;

using Microsoft.AspNetCore.WebUtilities;

using Newtonsoft.Json;

using Scruffy.Data.Json.MediaWiki;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Commands;

/// <summary>
/// Searching the web
/// </summary>
[ModuleLifespan(ModuleLifespan.Transient)]
[BlockedChannelCheck]
[Group("search")]
[Aliases("se")]
[HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
public class SearchCommandModule : LocatedCommandModuleBase
{
    #region Command methods

    /// <summary>
    /// Searching google
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <param name="searchTerm">Search term</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command("google")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public Task Google(CommandContext commandContext, [RemainingText] string searchTerm)
    {
        return InvokeAsync(commandContext,
                           async commandContextContainer =>
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
                                   request.Q = searchTerm;
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

                                       embedBuilder.WithThumbnail("https://cdn.discordapp.com/attachments/847555191842537552/861182135000236032/google.png");
                                       embedBuilder.WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64");
                                       embedBuilder.WithTimestamp(DateTime.Now);

                                       await commandContext.RespondAsync(embedBuilder)
                                                           .ConfigureAwait(false);
                                   }
                                   else
                                   {
                                       await commandContext.RespondAsync(LocalizationGroup.GetText("NoResults", "I couldn't find anything."))
                                                           .ConfigureAwait(false);
                                   }
                               }
                           });
    }

    /// <summary>
    /// Searching google
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <param name="searchTerm">Search term</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command("gw2wiki")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public Task GW2Wiki(CommandContext commandContext, [RemainingText] string searchTerm)
    {
        return InvokeAsync(commandContext,
                           async commandContextContainer =>
                           {
                               var embedBuilder = new DiscordEmbedBuilder
                                                  {
                                                      Color = DiscordColor.Green
                                                  };

                               embedBuilder.WithTitle(LocalizationGroup.GetText("SearchResults", "Search results"));

                               var client = HttpClientFactory.CreateClient();

                               using (var response = await client.GetAsync(QueryHelpers.AddQueryString("https://wiki.guildwars2.com/api.php",
                                                                                                       new Dictionary<string, string>
                                                                                                       {
                                                                                                           ["action"] = "query",
                                                                                                           ["srwhat"] = "title",
                                                                                                           ["list"] = "search",
                                                                                                           ["format"] = "json",
                                                                                                           ["srsearch"] = searchTerm,
                                                                                                       }))
                                                                 .ConfigureAwait(false))
                               {
                                   var jsonResult = await response.Content
                                                                  .ReadAsStringAsync()
                                                                  .ConfigureAwait(false);

                                   var stringBuilder = new StringBuilder(1024);

                                   var searchResult = JsonConvert.DeserializeObject<SearchQueryRoot>(jsonResult);

                                   if (searchResult?.Query?.Search?.Count > 0)
                                   {
                                       foreach (var result in searchResult.Query.Search)
                                       {
                                           var current = "> " + Formatter.MaskedUrl(result.Title, new Uri("https://wiki.guildwars2.com/?curid=" + result.PageId)) + "\n";

                                           if (current.Length + stringBuilder.Length > stringBuilder.Capacity)
                                           {
                                               break;
                                           }

                                           stringBuilder.Append(current);
                                       }
                                   }

                                   stringBuilder.Append("\u200B");

                                   embedBuilder.AddField(LocalizationGroup.GetText("TitleSearch", "Title search"), stringBuilder.ToString());
                               }

                               using (var response = await client.GetAsync(QueryHelpers.AddQueryString("https://wiki.guildwars2.com/api.php",
                                                                                                       new Dictionary<string, string>
                                                                                                       {
                                                                                                           ["action"] = "query",
                                                                                                           ["srwhat"] = "text",
                                                                                                           ["list"] = "search",
                                                                                                           ["format"] = "json",
                                                                                                           ["srsearch"] = searchTerm,
                                                                                                       }))
                                                                 .ConfigureAwait(false))
                               {
                                   var jsonResult = await response.Content
                                                                  .ReadAsStringAsync()
                                                                  .ConfigureAwait(false);

                                   var stringBuilder = new StringBuilder(1024);

                                   var searchResult = JsonConvert.DeserializeObject<SearchQueryRoot>(jsonResult);

                                   if (searchResult?.Query?.Search?.Count > 0)
                                   {
                                       foreach (var result in searchResult.Query.Search)
                                       {
                                           var current = "> " + Formatter.MaskedUrl(result.Title, new Uri("https://wiki.guildwars2.com/?curid=" + result.PageId)) + "\n";

                                           if (current.Length + stringBuilder.Length > stringBuilder.Capacity)
                                           {
                                               break;
                                           }

                                           stringBuilder.Append(current);
                                       }
                                   }

                                   stringBuilder.Append("\u200B");

                                   embedBuilder.AddField(LocalizationGroup.GetText("TextSearch", "Text search"), stringBuilder.ToString());
                               }

                               embedBuilder.WithThumbnail("https://media.discordapp.net/attachments/847555191842537552/861182143987712010/gw2.png");
                               embedBuilder.WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64");
                               embedBuilder.WithTimestamp(DateTime.Now);

                               await commandContextContainer.Message
                                                            .RespondAsync(embedBuilder)
                                                            .ConfigureAwait(false);
                           });
    }

    #endregion // Command methods
}
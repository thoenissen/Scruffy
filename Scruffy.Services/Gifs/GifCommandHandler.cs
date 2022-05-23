using System.Net.Http;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Json.Tenor;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.Services.Gifs;

/// <summary>
/// Gif related commands
/// </summary>
public class GifCommandHandler : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// <see cref="HttpClient"/>-Factory
    /// </summary>
    private IHttpClientFactory _httpClientFactory;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="httpClientFactory">http Client Factory</param>
    public GifCommandHandler(LocalizationService localizationService, IHttpClientFactory httpClientFactory)
        : base(localizationService)
    {
        _httpClientFactory = httpClientFactory;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Creation of a one time reminder
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <param name="searchTerm">gif search term</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task Gif(IContextContainer commandContext, string searchTerm)
    {
        var rnd = new Random(DateTime.Now.Millisecond);

        var client = _httpClientFactory.CreateClient();

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
                var processingMessage = await commandContext.DeferProcessing()
                                                            .ConfigureAwait(false);

                await commandContext.Channel
                                    .SendMessageAsync(searchResult.Results[rnd.Next(searchResult.Results.Count - 1)]
                                                                  .ItemUrl)
                                    .ConfigureAwait(false);

                await processingMessage.DeleteAsync()
                                       .ConfigureAwait(false);

                LoggingService.AddServiceLogEntry(LogEntryLevel.Information, nameof(GifCommandHandler), "Post random gif", searchTerm);
            }
        }
    }

    #endregion // Methods
}
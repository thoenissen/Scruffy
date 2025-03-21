using System.Net.Http;

using Newtonsoft.Json;

using Scruffy.Data.Enumerations.DpsReport;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Json.DpsReport;
using Scruffy.Services.Core;

namespace Scruffy.Services.WebApi;

/// <summary>
/// DPS-Report Connector
/// </summary>
public class DpsReportConnector
{
    #region Fields

    /// <summary>
    /// Client factory
    /// </summary>
    private readonly IHttpClientFactory _clientFactory;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="clientFactory">Client factory</param>
    public DpsReportConnector(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Requests a filtered list of DPS reports
    /// </summary>
    /// <param name="token">DPS-report user token</param>
    /// <param name="filter">Function to filter reports</param>
    /// <param name="shouldAbort">Function to abort searching further</param>
    /// <param name="skipEnhancement">Whether to skip certain enhancements on the upload</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public IAsyncEnumerable<Upload> GetUploads(string token, Func<Upload, bool> filter, Func<Upload, bool> shouldAbort, bool skipEnhancement = false)
    {
        var omitTime = new DateTimeOffset(DateTime.MinValue, TimeSpan.Zero);
        return GetUploads(token, omitTime, omitTime, filter, shouldAbort, skipEnhancement);
    }

    /// <summary>
    /// Requests a filtered list of DPS reports
    /// </summary>
    /// <param name="token">DPS-report user token</param>
    /// <param name="startTime">Date for the oldest report to get. Set to zero to omit this parameter.</param>
    /// <param name="endTime">Date for the most recent reports to get. Set to zero to omit this parameter.</param>
    /// <param name="filter">Function to filter reports</param>
    /// <param name="shouldAbort">Function to abort searching further</param>
    /// <param name="skipEnhancement">Whether to skip certain enhancements on the upload</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async IAsyncEnumerable<Upload> GetUploads(string token, DateTimeOffset startTime, DateTimeOffset? endTime, Func<Upload, bool> filter = null, Func<Upload, bool> shouldAbort = null, bool skipEnhancement = false)
    {
        var firstPage = await GetUploads(token, 1, startTime, endTime).ConfigureAwait(false);

        if (firstPage != null)
        {
            var uploadTasks = new List<Task<Upload>>();

            if (shouldAbort == null)
            {
                var pageTasks = new List<Task<Page>> { Task.FromResult(firstPage) };

                for (var i = 2; i <= firstPage.Pages; i++)
                {
                    pageTasks.Add(GetUploads(token, i, startTime, endTime));
                }

                while (pageTasks.Count != 0)
                {
                    var finishedTask = await Task.WhenAny(pageTasks).ConfigureAwait(false);
                    pageTasks.Remove(finishedTask);
                    var page = finishedTask.Result;

                    if (page.Uploads != null)
                    {
                        uploadTasks.AddRange(page.Uploads.Where(upload => CouldBeValidUpload(filter, upload)).Select(upload => CheckUpload(upload, skipEnhancement)));
                    }
                }
            }
            else
            {
                var currentPage = 2;
                var pageCount = firstPage.Pages;

                Page page;
                var nextPage = Task.FromResult(firstPage);

                do
                {
                    await nextPage.ConfigureAwait(false);
                    page = nextPage.Result;

                    nextPage = GetUploads(token, currentPage, startTime, endTime);
                    ++currentPage;

                    if (page?.Uploads != null)
                    {
                        foreach (var upload in page.Uploads)
                        {
                            if (shouldAbort(upload))
                            {
                                currentPage = pageCount;
                                break;
                            }

                            if (CouldBeValidUpload(filter, upload))
                            {
                                uploadTasks.Add(CheckUpload(upload, skipEnhancement));
                            }
                        }
                    }
                }
                while (page?.Uploads != null && currentPage <= pageCount);
            }

            while (uploadTasks.Count != 0)
            {
                var finishedTask = await Task.WhenAny(uploadTasks).ConfigureAwait(false);
                uploadTasks.Remove(finishedTask);

                if (finishedTask.Result != null)
                {
                    yield return finishedTask.Result;
                }
            }
        }
    }

    /// <summary>
    /// Request DPS reports
    /// </summary>
    /// <param name="userToken">User token</param>
    /// <param name="page">Page</param>
    /// <param name="startTime">Date for the oldest report to get. Set to zero to omit this parameter.</param>
    /// <param name="endTime">Date for the most recent reports to get. Set to zero to omit this parameter.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task<Page> GetUploads(string userToken, int page, DateTimeOffset startTime, DateTimeOffset? endTime)
    {
        var url = $"https://dps.report/getUploads?userToken={userToken}&page={page}";

        if (startTime.Ticks > 0)
        {
            url += $"&sinceEncounter={startTime.ToUnixTimeSeconds()}";
        }

        /* This doesn't work at the moment, see code below for work-around
        if (endTime?.Ticks > 0)
        {
            url += $"&untilEncounter={endTime.ToUnixTimeSeconds()}";
        }
        */

        var client = _clientFactory.CreateClient();

        using var response = await client.GetAsync(url).ConfigureAwait(false);
        var jsonResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var parsedPage = JsonConvert.DeserializeObject<Page>(jsonResult);

        if (endTime != null && parsedPage?.Uploads != null)
        {
            parsedPage.Uploads = parsedPage.Uploads.Where(upload => new DateTimeOffset(upload.EncounterTime, TimeSpan.Zero) <= endTime).ToList();
        }

        return parsedPage;
    }

    /// <summary>
    /// Returns whether a upload could be valid
    /// </summary>
    /// <param name="filter">Function to filter reports</param>
    /// <param name="upload">Upload</param>
    /// <returns>Whether the given upload could be valid</returns>
    private bool CouldBeValidUpload(Func<Upload, bool> filter, Upload upload)
    {
        return (upload.Encounter.Success || upload.Encounter.Duration.TotalSeconds > 30) && (filter == null || filter(upload));
    }

    /// <summary>
    /// Checks the upload data whether it's complete or has to be updated with the full JSON data
    /// </summary>
    /// <param name="upload">Upload to check</param>
    /// <param name="skipEnhancement">Whether to skip certain enhancements on the upload</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    private async Task<Upload> CheckUpload(Upload upload, bool skipEnhancement)
    {
        // HACK
        // Sometimes the API doesn't report all players, so we have to load the full report for correct data
        // We also need to get the fight name to differentiate the different Ai phases.
        if (skipEnhancement == false &&
            (upload.Players.Count != upload.Encounter.NumberOfPlayers || upload.Encounter.BossId == 23254))
        {
            await UpdateUploadData(upload).ConfigureAwait(false);
        }

        return skipEnhancement || upload.Players.Count > 0 ? upload : null;
    }

    /// <summary>
    /// Refresh the player data from the specific upload
    /// </summary>
    /// <param name="upload">Upload to be refreshed</param>
    /// <returns>The enriched upload</returns>
    private async Task UpdateUploadData(Upload upload)
    {
        if (upload.Encounter.JsonAvailable)
        {
            var log = await GetLog(upload.Id).ConfigureAwait(false);

            upload.FightName = log.FightName;
            upload.Players = new Dictionary<string, Player>();

            foreach (var player in log.Players)
            {
                if (upload.Players.TryAdd(player.DisplayName, player) == false)
                {
                    upload.Players.Add($"Unknown ({Guid.NewGuid()})", player);

                    LoggingService.AddServiceLogEntry(LogEntryLevel.Error,
                                                      nameof(DpsReportConnector),
                                                      "Duplicate player display name",
                                                      null,
                                                      new
                                                      {
                                                          player.DisplayName,
                                                          upload.Id,
                                                          upload.Permalink,
                                                      });
                }
            }
        }
    }

    /// <summary>
    /// Requests the log for the given upload id
    /// </summary>
    /// <param name="id">The Id of the upload</param>
    /// <returns>The log for the given id</returns>
    public async Task<Log> GetLog(string id)
    {
        var client = _clientFactory.CreateClient();

        using (var response = await client.GetAsync($"https://dps.report/getJson?id={id}")
                                         .ConfigureAwait(false))
        {
            var jsonResult = await response.Content
                                           .ReadAsStringAsync()
                                           .ConfigureAwait(false);

            return JsonConvert.DeserializeObject<Log>(jsonResult);
        }
    }

    /// <summary>
    /// Determines the report type of a given boss
    /// </summary>
    /// <param name="bossId">The ID of the boss to determine the type</param>
    /// <returns>Report type of the boss</returns>
    public DpsReportType GetReportType(int bossId)
    {
        return GetReportGroup(bossId).GetReportType();
    }

    /// <summary>
    /// Determines the report group of a given boss
    /// </summary>
    /// <param name="bossId">The ID of the boss to determine the group</param>
    /// <returns>report group of the boss</returns>
    public DpsReportGroup GetReportGroup(int bossId)
    {
        switch (bossId)
        {
            case 17021:
            case 17028:
            case 16948:
                return DpsReportGroup.Nightmare;

            case 17632:
            case 17949:
            case 17759:
                return DpsReportGroup.ShatteredObservatory;

            case 23254:
                return DpsReportGroup.SunquaPeak;

            case 25577:
                return DpsReportGroup.SilentSurf;

            case 26257:
            case 26231:
                return DpsReportGroup.LonelyTower;

            case 22154:
            case 22343:
            case 22481:
            case 22492:
            case 22436:
            case 22711:
            case 22836:
            case 22521:
                return DpsReportGroup.IBSStrikes;

            case 24033:
            case 24768:
            case 25247:
            case 23957:
            case 24485:
            case 24266:
            case 43488:
            case 1378:
            case 24375:
            case 25414:
                return DpsReportGroup.EoDStrikes;

            case 25705:
            case 25989:
                return DpsReportGroup.SotOStrikes;

            case 16169:
            case 16202:
            case 16178:
            case 16198:
            case 16177:
            case 16199:
            case 19676:
            case 19645:
            case 16174:
            case 16176:
                return DpsReportGroup.TrainingArea;

            case 15438:
            case 15429:
            case 15375:
                return DpsReportGroup.SpritVale;

            case 16123:
            case 16088:
            case 16137:
            case 16125:
            case 16115:
                return DpsReportGroup.SalvationPass;

            case 16253:
            case 16235:
            case 16247:
            case 16246:
                return DpsReportGroup.StrongholdOfTheFaithful;

            case 17194:
            case 17172:
            case 17188:
            case 17154:
                return DpsReportGroup.BastionOfThePenitent;

            case 19767:
            case 19828:
            case 19691:
            case 19536:
            case 19651:
            case 19844:
            case 19450:
                return DpsReportGroup.HallOfChains;

            case 43974:
            case 10142: // Gadget
            case 37464: // Gadget
            case 21105:
            case 21089:
            case 20934:
                return DpsReportGroup.MythwrightGambit;

            case 22006:
            case 21964:
            case 22000:
                return DpsReportGroup.TheKeyOfAhdashim;

            case 26774:
            case 26725:
            case 26712:
                return DpsReportGroup.MountBalrior;

            default:
                return DpsReportGroup.Unknown;
        }
    }

    /// <summary>
    /// Determines the sort value for a given boss
    /// </summary>
    /// <param name="bossId">The ID of the boss to determine the sort value</param>
    /// <returns>The sort value for the given boss</returns>
    public int GetSortValue(int bossId)
    {
        int bossSortValue;

        switch (bossId)
        {
            case 17021:
            case 17632:
            case 23254:
            case 22154:
            case 22343:
            case 22492:
            case 22711:
            case 22836:
            case 24033:
            case 23957:
            case 24485:
            case 43488:
            case 16169:
            case 15438:
            case 16123:
            case 16253:
            case 17194:
            case 19767:
            case 43974:
            case 22006:
            case 25705:
            case 26774:
                bossSortValue = 1;
                break;

            case 17028:
            case 17949:
            case 22481:
            case 22436:
            case 22521:
            case 24768:
            case 24266:
            case 1378:
            case 16202:
            case 15429:
            case 16088:
            case 16235:
            case 17172:
            case 17188:
            case 19828:
            case 10142:
            case 21964:
            case 25989:
            case 26725:
                bossSortValue = 2;
                break;

            case 16948:
            case 17759:
            case 25247:
            case 24375:
            case 16178:
            case 15375:
            case 16137:
            case 16247:
            case 17154:
            case 19691:
            case 37464:
            case 22000:
            case 26712:
                bossSortValue = 3;
                break;

            case 16198:
            case 16125:
            case 16246:
            case 19536:
            case 21105:
                bossSortValue = 4;
                break;

            case 16177:
            case 16115:
            case 19651:
            case 21089:
            case 25414:
                bossSortValue = 5;
                break;

            case 16199:
            case 19844:
            case 20934:
                bossSortValue = 6;
                break;

            case 19676:
            case 19450:
                bossSortValue = 7;
                break;

            case 19645:
                bossSortValue = 8;
                break;

            case 16174:
                bossSortValue = 9;
                break;

            case 16176:
                bossSortValue = 10;
                break;

            default:
                bossSortValue = 0;
                break;
        }

        return GetReportGroup(bossId).GetSortValue() + bossSortValue;
    }

    /// <summary>
    /// Get boss icon ID
    /// </summary>
    /// <param name="bossId">Boss ID</param>
    /// <returns>Icon ID</returns>
    public ulong GetRaidBossIconId(int bossId)
    {
        ulong iconId = bossId switch
                              {
                                  15438 => 848910035747864576,
                                  15429 => 848908993538949131,
                                  15375 => 848909543915651072,
                                  16123 => 848909627982610482,
                                  16088 => 848909882115358720,
                                  16137 => 848909882115358720,
                                  16125 => 848909882115358720,
                                  16115 => 848909162821845043,
                                  16253 => 743938372195844117,
                                  16235 => 848909049599885322,
                                  16247 => 848909953112473622,
                                  16246 => 848910090370940949,
                                  17194 => 848908521680142359,
                                  17172 => 848909340827713557,
                                  17188 => 848909587938803762,
                                  17154 => 848908773996101642,
                                  19767 => 848911345964679188,
                                  19828 => 743940484455596064,
                                  19651 => 848909739509547058,
                                  19844 => 848909739509547058,
                                  19536 => 848908876039585822,
                                  19691 => 848908317832773692,
                                  19450 => 848908828866379777,
                                  43974 => 848908712692547614,
                                  21105 => 848909098619895808,
                                  21089 => 848909098619895808,
                                  20934 => 848909410691973140,
                                  22006 => 848908580749049866,
                                  21964 => 848908653637533736,
                                  22000 => 848909465553207296,
                                  _ => 0ul
                              };

        return iconId;
    }

    #endregion // Methods
}
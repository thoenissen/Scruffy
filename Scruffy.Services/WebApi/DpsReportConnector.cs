using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;

using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

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

    /// <summary>
    /// Minio client factory to create clients for Minio operations
    /// </summary>
    private readonly IMinioClientFactory _minioClientFactory;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="clientFactory">Client factory</param>
    /// <param name="minioClientFactory">Minio factory</param>
    public DpsReportConnector(IHttpClientFactory clientFactory, IMinioClientFactory minioClientFactory)
    {
        _clientFactory = clientFactory;
        _minioClientFactory = minioClientFactory;
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
        var isUploadComplete = true;

        // HACK
        // Sometimes the API doesn't report all players, so we have to load the full report for correct data
        // We also need to get the fight name to differentiate the different Ai phases.
        if (skipEnhancement == false
            && (upload.Players.Count != upload.Encounter.NumberOfPlayers
                || upload.Encounter.BossId == 23254))
        {
            isUploadComplete = await UpdateUploadData(upload).ConfigureAwait(false);
        }

        return isUploadComplete
                   ? upload
                   : null;
    }

    /// <summary>
    /// Refresh the player data from the specific upload
    /// </summary>
    /// <param name="upload">Upload to be refreshed</param>
    /// <returns>Could the data be updated?</returns>
    private async Task<bool> UpdateUploadData(Upload upload)
    {
        if (upload.Encounter.JsonAvailable == false)
        {
            return false;
        }

        var log = await GetLog(upload.Id).ConfigureAwait(false);

        if (log == null)
        {
            return false;
        }

        upload.FightName = log.FightName;
        upload.Players = [];

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

        return true;
    }

    /// <summary>
    /// Requests the log for the given upload id
    /// </summary>
    /// <param name="id">The ID of the upload</param>
    /// <returns>The log for the given id</returns>
    public async Task<Log> GetLog(string id)
    {
        return await TryGetLogFromCache(id).ConfigureAwait(false)
               ?? await GetLogFromDpsReport(id).ConfigureAwait(false);
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
        return bossId switch
               {
                   17021
                   or 17028
                   or 16948 => DpsReportGroup.Nightmare,

                   17632
                   or 17949
                   or 17759 => DpsReportGroup.ShatteredObservatory,

                   23254 => DpsReportGroup.SunquaPeak,
                   25577 => DpsReportGroup.SilentSurf,

                   26257
                   or 26231 => DpsReportGroup.LonelyTower,

                   27010 => DpsReportGroup.Kinfall,

                   22154
                   or 22343
                   or 22481
                   or 22492
                   or 22436
                   or 22711
                   or 22836
                   or 22521 => DpsReportGroup.IBSStrikes,

                   24033
                   or 24768
                   or 25247
                   or 23957
                   or 24485
                   or 24266
                   or 43488
                   or 1378
                   or 24375
                   or 25414 => DpsReportGroup.EoDStrikes,

                   25705
                   or 25989 => DpsReportGroup.SotOStrikes,

                   16169
                   or 16202
                   or 16178
                   or 16198
                   or 16177
                   or 16199
                   or 19676
                   or 19645
                   or 16174
                   or 16176 => DpsReportGroup.TrainingArea,

                   15438
                   or 15429
                   or 15375 => DpsReportGroup.SpritVale,

                   16123
                   or 16088
                   or 16137
                   or 16125
                   or 16115 => DpsReportGroup.SalvationPass,

                   16253
                   or 16235
                   or 16247
                   or 16246 => DpsReportGroup.StrongholdOfTheFaithful,

                   17194
                   or 17172
                   or 17188
                   or 17154 => DpsReportGroup.BastionOfThePenitent,

                   19767
                   or 19828
                   or 19691
                   or 19536
                   or 19651
                   or 19844
                   or 19450 => DpsReportGroup.HallOfChains,

                   43974
                   or 10142
                   or 37464
                   or 21105
                   or 21089
                   or 20934 => DpsReportGroup.MythwrightGambit,

                   22006
                   or 21964
                   or 22000 => DpsReportGroup.TheKeyOfAhdashim,

                   26774
                   or 26725
                   or 26712 => DpsReportGroup.MountBalrior,

                   _ => DpsReportGroup.Unknown
               };
    }

    /// <summary>
    /// Determines the sort value for a given boss
    /// </summary>
    /// <param name="bossId">The ID of the boss to determine the sort value</param>
    /// <returns>The sort value for the given boss</returns>
    public int GetSortValue(int bossId)
    {
        var bossSortValue = bossId switch
                            {
                                17021
                                or 17632
                                or 23254
                                or 22154
                                or 22343
                                or 22492
                                or 22711
                                or 22836
                                or 24033
                                or 23957
                                or 24485
                                or 43488
                                or 16169
                                or 15438
                                or 16123
                                or 16253
                                or 17194
                                or 19767
                                or 43974
                                or 22006
                                or 25705
                                or 26774 => 1,

                                17028
                                or 17949
                                or 22481
                                or 22436
                                or 22521
                                or 24768
                                or 24266
                                or 1378
                                or 16202
                                or 15429
                                or 16088
                                or 16235

                                or 17172
                                or 17188
                                or 19828
                                or 10142
                                or 21964
                                or 25989
                                or 26725 => 2,

                                16948
                                or 17759
                                or 25247
                                or 24375
                                or 16178
                                or 15375
                                or 16137
                                or 16247
                                or 17154
                                or 19691
                                or 37464
                                or 22000
                                or 26712 => 3,

                                16198
                                or 16125
                                or 16246
                                or 19536
                                or 21105 => 4,

                                16177
                                or 16115
                                or 19651
                                or 21089
                                or 25414 => 5,

                                16199
                                or 19844
                                or 20934 => 6,

                                19676
                                or 19450 => 7,

                                19645 => 8,
                                16174 => 9,
                                16176 => 10,
                                _ => 0
                            };

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

    /// <summary>
    /// Tries to get the log for the cache
    /// </summary>
    /// <param name="id">The ID of the upload</param>
    /// <returns>The log for the given id</returns>
    [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "Object is not disposed when being used.")]
    private async Task<Log> TryGetLogFromCache(string id)
    {
        Log log = null;

        try
        {
            using (var minioClient = _minioClientFactory.CreateClient())
            {
                using (var memoryStream = new MemoryStream())
                {
                    var e = new GetObjectArgs().WithBucket("dps.report")
                                               .WithObject($"{id}.json")
                                               .WithCallbackStream(objectStream => objectStream.CopyTo(memoryStream));

                    await minioClient.GetObjectAsync(e).ConfigureAwait(false);

                    memoryStream.Position = 0;

                    using (var reader = new StreamReader(memoryStream))
                    {
                        var logContent = await reader.ReadToEndAsync().ConfigureAwait(false);

                        log = JsonConvert.DeserializeObject<Log>(logContent);
                    }
                }
            }
        }
        catch (ObjectNotFoundException)
        {
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(DpsReportConnector), $"An error occurred while trying to get the detailed DPS report from cache for ID {id}.", null, ex);
        }

        return log;
    }

    /// <summary>
    /// Request log from dps.report
    /// </summary>
    /// <param name="id">The ID of the upload</param>
    /// <returns>The log for the given id</returns>
    private async Task<Log> GetLogFromDpsReport(string id)
    {
        using (var client = _clientFactory.CreateClient())
        {
            try
            {
                using (var response = await client.GetAsync($"https://dps.report/getJson?id={id}")
                           .ConfigureAwait(false))
                {
                    var jsonResult = await response.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);

                    await UploadToCache(jsonResult, id).ConfigureAwait(false);

                    return JsonConvert.DeserializeObject<Log>(jsonResult);
                }
            }
            catch (Exception ex)
            {
                LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(DpsReportConnector), $"An error occurred while trying to download the detailed DPS report for ID {id}.", null, ex);
            }
        }

        return null;
    }

    /// <summary>
    /// Uploads the detailed DPS report to the cache
    /// </summary>
    /// <param name="content">Content</param>
    /// <param name="id">ID</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task UploadToCache(string content, string id)
    {
        try
        {
            using (var minioClient = _minioClientFactory.CreateClient())
            {
                var data = Encoding.UTF8.GetBytes(content);

                using (var memoryStream = new MemoryStream(data))
                {
                    var e = new PutObjectArgs().WithBucket("dps.report")
                                               .WithObject($"{id}.json")
                                               .WithStreamData(memoryStream)
                                               .WithObjectSize(data.Length)
                                               .WithContentType("application/json");

                    await minioClient.PutObjectAsync(e).ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(DpsReportConnector), $"An error occurred while trying to upload the detailed DPS report to cache for ID {id}.", null, ex);
        }
    }

    #endregion // Methods
}
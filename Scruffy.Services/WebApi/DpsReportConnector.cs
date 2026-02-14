using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;

using GW2EIEvtcParser;

using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

using Newtonsoft.Json;

using Scruffy.Data.Enumerations.DpsReport;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Json.DpsReport;
using Scruffy.Services.Core;
using Scruffy.Services.GuildWars2;

namespace Scruffy.Services.WebApi;

/// <summary>
/// DPS-Report Connector
/// </summary>
public class DpsReportConnector
{
    #region Fields

    /// <summary>
    /// JSON serializer settings
    /// </summary>
    private static readonly JsonSerializerSettings _settings = new()
                                                   {
                                                       NullValueHandling = NullValueHandling.Ignore,
                                                   };

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
    /// Get upload per page
    /// </summary>
    /// <param name="userToken">User token</param>
    /// <param name="page">Page</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<Page> GetUploads(string userToken, int page)
    {
        using (var client = _clientFactory.CreateClient())
        {
            using (var response = await client.GetAsync($"https://dps.report/getUploads?userToken={userToken}&page={page}")
                                              .ConfigureAwait(false))
            {
                var jsonResult = await response.Content
                                               .ReadAsStringAsync()
                                               .ConfigureAwait(false);

                return JsonConvert.DeserializeObject<Page>(jsonResult, _settings);
            }
        }
    }

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
        var parsedPage = JsonConvert.DeserializeObject<Page>(jsonResult, _settings);

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
                || SpeciesIDs.GetTargetID(upload.Encounter.BossId) == SpeciesIDs.TargetID.AiKeeperOfThePeak))
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
    /// Requests the log for the given upload Id
    /// </summary>
    /// <param name="id">The ID of the upload</param>
    /// <returns>The log for the given Id</returns>
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
        return DpsReportAnalyzer.GetReportGroupByBossId(bossId).GetReportType();
    }

    /// <summary>
    /// Determines the sort value for a given boss
    /// </summary>
    /// <param name="bossId">The ID of the boss to determine the sort value</param>
    /// <returns>The sort value for the given boss</returns>
    public int GetSortValue(int bossId)
    {
        return DpsReportAnalyzer.GetReportGroupByBossId(bossId).GetSortValue()
               + DpsReportAnalyzer.GetBossOrder(bossId);
    }

    /// <summary>
    /// Tries to get the log for the cache
    /// </summary>
    /// <param name="id">The ID of the upload</param>
    /// <returns>The log for the given Id</returns>
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

                        log = JsonConvert.DeserializeObject<Log>(logContent, _settings);
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
    /// <returns>The log for the given Id</returns>
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

                    return JsonConvert.DeserializeObject<Log>(jsonResult, _settings);
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
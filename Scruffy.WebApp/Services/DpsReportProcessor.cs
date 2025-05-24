using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using GW2EIJSON;

using Microsoft.Extensions.Logging;

using Minio.AspNetCore;
using Minio.DataModel.Args;
using Minio.Exceptions;

using Scruffy.WebApp.Services.Data;

namespace Scruffy.WebApp.Services;

/// <summary>
/// Processes requests for detailed DPS reports from dps.report
/// </summary>
public sealed class DpsReportProcessor : IAsyncDisposable
{
    #region Fields

    /// <summary>
    /// HTTP client factory to create clients for requests
    /// </summary>
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Minio client factory to create clients for Minio operations
    /// </summary>
    private readonly IMinioClientFactory _minioClientFactory;

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger<DpsReportProcessor> _logger;

    /// <summary>
    /// Channel for processing detailed DPS report requests
    /// </summary>
    private readonly Channel<DetailedDpsReportRequest> _channel;

    /// <summary>
    /// Tasks for processing requests concurrently
    /// </summary>
    private readonly Task[] _processorTask;

    /// <summary>
    /// Cancellation token source to cancel processing tasks
    /// </summary>
    private readonly CancellationTokenSource _tokenSource;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpClientFactory">HTTP client factory to create clients for requests</param>
    /// <param name="minioClientFactory">Minio client factory to create clients for Minio operations</param>
    /// <param name="logger">Logger for logging</param>
    public DpsReportProcessor(IHttpClientFactory httpClientFactory, IMinioClientFactory minioClientFactory, ILogger<DpsReportProcessor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _minioClientFactory = minioClientFactory;
        _logger = logger;
        _channel = Channel.CreateUnbounded<DetailedDpsReportRequest>();
        _tokenSource = new CancellationTokenSource();

        _processorTask = new Task[Environment.ProcessorCount];

        for (var i = 0; i < _processorTask.Length; i++)
        {
            _processorTask[i] = ProcessRequestsAsync();
        }
    }

    #endregion // Constructor

    #region Public methods

    /// <summary>
    /// Gets a detailed DPS report by its ID
    /// </summary>
    /// <param name="id">ID</param>
    /// <returns>Json log</returns>
    public Task<JsonLog> Get(string id)
    {
        var request = new DetailedDpsReportRequest
                      {
                          Id = id,
                          Report = new TaskCompletionSource<JsonLog>(TaskCreationOptions.RunContinuationsAsynchronously)
                      };

        _channel.Writer.TryWrite(request);

        return request.Report.Task;
    }

    #endregion // Public methods

    #region Private methods

    /// <summary>
    /// Processes requests for detailed DPS reports asynchronously
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task ProcessRequestsAsync()
    {
        var token = _tokenSource.Token;

        while (token.IsCancellationRequested == false)
        {
            try
            {
                var request = await _channel.Reader.ReadAsync(token).ConfigureAwait(false);

                await ProcessRequest(request).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing a detailed DPS report request.");
            }
        }
    }

    /// <summary>
    /// Processes a single detailed DPS report request
    /// </summary>
    /// <param name="request">Request</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task ProcessRequest(DetailedDpsReportRequest request)
    {
        if (await TryGetFromCache(request).ConfigureAwait(false) == false)
        {
            await GetFromDpsReport(request).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Tries to get a detailed DPS report from the cache
    /// </summary>
    /// <param name="request">Request</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task<bool> TryGetFromCache(DetailedDpsReportRequest request)
    {
        var success = false;

        try
        {
            using (var minioClient = _minioClientFactory.CreateClient())
            {
                using (var memoryStream = new MemoryStream())
                {
                    var e = new GetObjectArgs().WithBucket("dps.report")
                                               .WithObject(request.Id)
                                               .WithCallbackStream(objectStream => objectStream.CopyTo(memoryStream));

                    await minioClient.GetObjectAsync(e).ConfigureAwait(false);

                    memoryStream.Position = 0;

                    var log = JsonSerializer.Deserialize(memoryStream, JsonLogSerializerContext.Default.JsonLog);

                    request.Report.SetResult(log);

                    success = true;
                }
            }
        }
        catch (ObjectNotFoundException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while trying to get the detailed DPS report from cache for ID {Id}.", request.Id);
        }

        return success;
    }

    /// <summary>
    /// Gets a detailed DPS report from dps.report by its ID
    /// </summary>
    /// <param name="request">Request</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task GetFromDpsReport(DetailedDpsReportRequest request)
    {
        using (var client = _httpClientFactory.CreateClient())
        {
            var response = await client.GetAsync($"https://dps.report/getJson?id={request.Id}").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content
                                            .ReadAsStringAsync()
                                            .ConfigureAwait(false);

                var log = JsonSerializer.Deserialize(content, JsonLogSerializerContext.Default.JsonLog);

                request.Report.SetResult(log);

                await UploadToCache(content, request.Id).ConfigureAwait(false);
            }
            else
            {
                request.Report.SetResult(null);
            }
        }
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
                                               .WithObject(id)
                                               .WithStreamData(memoryStream)
                                               .WithObjectSize(data.Length)
                                               .WithContentType("application/json");

                    await minioClient.PutObjectAsync(e).ConfigureAwait(false);
                }
            }
        }
        catch (ObjectNotFoundException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while trying to upload the detailed DPS report to cache for ID {Id}.", id);
        }
    }

    #endregion // Private methods

    #region IAsyncDisposable

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        _tokenSource?.Dispose();

        await Task.WhenAll(_processorTask).ConfigureAwait(false);
    }

    #endregion // IAsyncDisposable
}
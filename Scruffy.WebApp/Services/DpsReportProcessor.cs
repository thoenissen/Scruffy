using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using GW2EIJSON;

using Microsoft.Extensions.Logging;

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
    private readonly IHttpClientFactory _factory;

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
    /// <param name="factory">HTTP client factory to create clients for requests</param>
    /// <param name="logger">Logger for logging</param>
    public DpsReportProcessor(IHttpClientFactory factory, ILogger<DpsReportProcessor> logger)
    {
        _factory = factory;
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
        using (var client = _factory.CreateClient())
        {
            var response = await client.GetAsync($"https://dps.report/getJson?id={request.Id}").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync(JsonLogSerializerContext.Default.JsonLog).ConfigureAwait(false);

                request.Report.SetResult(json);
            }
            else
            {
                request.Report.SetResult(null);
            }
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
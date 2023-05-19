using System.IO;
using System.Net.Http;

using Newtonsoft.Json;

using Scruffy.Data.Json.QuickChart;

namespace Scruffy.Services.WebApi;

/// <summary>
/// QuickChart.io - Connector
/// </summary>
public sealed class QuickChartConnector : IDisposable,
                                          IAsyncDisposable
{
    #region Fields

    /// <summary>
    /// Factory
    /// </summary>
    private readonly IHttpClientFactory _httpClientFactory;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpClientFactory">Http client factory</param>
    public QuickChartConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Request the account information
    /// </summary>
    /// <param name="data">Data</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<Stream> GetChartAsStream(ChartData data)
    {
        var client = _httpClientFactory.CreateClient();

        var jsonData = JsonConvert.SerializeObject(data,
                                                   new JsonSerializerSettings
                                                   {
                                                       NullValueHandling = NullValueHandling.Ignore
                                                   });

        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        using (var response = await client.PostAsync("https://quickchart.io:443/chart", content)
                                          .ConfigureAwait(false))
        {
            var memoryStream = new MemoryStream();

            var stream = await response.Content
                                       .ReadAsStreamAsync()
                                       .ConfigureAwait(false);

            await using (stream.ConfigureAwait(false))
            {
                await stream.CopyToAsync(memoryStream)
                            .ConfigureAwait(false);

                memoryStream.Position = 0;
            }

            return memoryStream;
        }
    }

    #endregion // Methods

    #region IDisposable

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
    }

    #endregion // IDisposable

    #region IAsyncDisposable

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    #endregion // IAsyncDisposable
}
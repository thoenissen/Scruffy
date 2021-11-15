using System;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Scruffy.Data.Json.ThatShaman;

namespace Scruffy.Services.WebApi;

/// <summary>
/// thatshaman.com connector
/// </summary>
public sealed class ThatShamanConnector : IAsyncDisposable, IDisposable
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
    public ThatShamanConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Get the next update
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<NextUpdateData> GetNextUpdate()
    {
        var client = _httpClientFactory.CreateClient();
        using (var response = await client.GetAsync("https://thatshaman.com/tools/countdown/?format=json")
                                          .ConfigureAwait(false))
        {
            var jsonResult = await response.Content
                                           .ReadAsStringAsync()
                                           .ConfigureAwait(false);

            return JsonConvert.DeserializeObject<NextUpdateData>(jsonResult);
        }
    }

    /// <summary>
    /// Get the next update
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<NextUpdateData> GetEODRelease()
    {
        var client = _httpClientFactory.CreateClient();
        using (var response = await client.GetAsync("https://thatshaman.com/tools/eod/?format=json")
                                          .ConfigureAwait(false))
        {
            var jsonResult = await response.Content
                                           .ReadAsStringAsync()
                                           .ConfigureAwait(false);

            return JsonConvert.DeserializeObject<NextUpdateData>(jsonResult);
        }
    }

    #endregion // Methods

    #region IAsyncDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    #endregion // IAsyncDisposable

    #region IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
    }

    #endregion // IDisposable
}
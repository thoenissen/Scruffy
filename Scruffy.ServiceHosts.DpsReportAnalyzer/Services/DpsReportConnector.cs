using Newtonsoft.Json;

using Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report;
using Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Upload;

namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Services;

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
    /// Request DPS reports
    /// </summary>
    /// <param name="userToken">User token</param>
    /// <param name="page">Page</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<Page> GetUploads(string userToken, int page)
    {
        var client = _clientFactory.CreateClient();

        using (var response = await client.GetAsync($"https://dps.report/getUploads?userToken={userToken}&page={page}")
                                          .ConfigureAwait(false))
        {
            var jsonResult = await response.Content
                                           .ReadAsStringAsync()
                                           .ConfigureAwait(false);

            return JsonConvert.DeserializeObject<Page>(jsonResult);
        }
    }

    /// <summary>
    /// Requests the log for the given upload id
    /// </summary>
    /// <param name="id">The Id of the upload</param>
    /// <returns>The log for the given id</returns>
    public async Task<JsonLog> GetLog(string id)
    {
        var client = _clientFactory.CreateClient();

        using (var response = await client.GetAsync($"https://dps.report/getJson?id={id}")
                                         .ConfigureAwait(false))
        {
            var jsonResult = await response.Content
                                           .ReadAsStringAsync()
                                           .ConfigureAwait(false);

            return JsonConvert.DeserializeObject<JsonLog>(jsonResult);
        }
    }

    #endregion // Methods
}
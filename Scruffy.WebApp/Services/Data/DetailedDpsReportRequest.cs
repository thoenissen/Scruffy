using System.Threading.Tasks;

using GW2EIJSON;

namespace Scruffy.WebApp.Services.Data;

/// <summary>
/// Request for a detailed DPS report
/// </summary>
public class DetailedDpsReportRequest
{
    /// <summary>
    /// ID
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Task completion source to report the result of the request
    /// </summary>
    public TaskCompletionSource<JsonLog> Report { get; init; }
}
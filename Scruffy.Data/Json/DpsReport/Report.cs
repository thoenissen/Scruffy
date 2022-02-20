using Newtonsoft.Json;

namespace Scruffy.Data.Json.DpsReport;

/// <summary>
/// Report
/// </summary>
public class Report
{
    /// <summary>
    /// Is anonymous?
    /// </summary>
    [JsonProperty("anonymous")]
    public bool Anonymous { get; set; }

    /// <summary>
    /// Is detailed?
    /// </summary>
    [JsonProperty("detailed")]
    public bool Detailed { get; set; }
}
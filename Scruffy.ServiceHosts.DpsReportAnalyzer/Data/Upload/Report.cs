using Newtonsoft.Json;

namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Upload;

/// <summary>
/// Report data
/// </summary>
public class Report
{
    /// <summary>
    /// Is the log anonymous?
    /// </summary>
    [JsonProperty("anonymous")]
    public bool Anonymous { get; set; }

    /// <summary>
    /// Are detailed log information available?
    /// </summary>
    [JsonProperty("detailed")]
    public bool Detailed { get; set; }
}
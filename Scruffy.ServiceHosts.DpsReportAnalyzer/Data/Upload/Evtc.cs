using Newtonsoft.Json;

namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Upload;

/// <summary>
/// EVTC
/// </summary>
public class Evtc
{
    /// <summary>
    /// Type
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// Version
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; set; }

    /// <summary>
    /// Boss ID
    /// </summary>
    [JsonProperty("bossId")]
    public int BossId { get; set; }
}
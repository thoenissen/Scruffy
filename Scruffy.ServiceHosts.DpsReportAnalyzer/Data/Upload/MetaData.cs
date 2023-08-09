using Newtonsoft.Json;

namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Upload;

/// <summary>
/// Meta data
/// </summary>
public class MetaData
{
    /// <summary>
    /// Id
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Perma link
    /// </summary>
    [JsonProperty("permalink")]
    public string Permalink { get; set; }

    /// <summary>
    /// Upload time
    /// </summary>
    [JsonProperty("uploadTime")]
    public int UploadTime { get; set; }

    /// <summary>
    /// Encounter time
    /// </summary>
    [JsonProperty("encounterTime")]
    public int EncounterTime { get; set; }

    /// <summary>
    /// Generator
    /// </summary>
    [JsonProperty("generator")]
    public string Generator { get; set; }

    /// <summary>
    /// Generator ID
    /// </summary>
    [JsonProperty("generatorId")]
    public int GeneratorId { get; set; }

    /// <summary>
    /// Generator version
    /// </summary>
    [JsonProperty("generatorVersion")]
    public int GeneratorVersion { get; set; }

    /// <summary>
    /// Language
    /// </summary>
    [JsonProperty("language")]
    public string Language { get; set; }

    /// <summary>
    /// Language ID
    /// </summary>
    [JsonProperty("languageId")]
    public int LanguageId { get; set; }

    /// <summary>
    /// EVTC
    /// </summary>
    [JsonProperty("evtc")]
    public Evtc Evtc { get; set; }

    /// <summary>
    /// Players
    /// </summary>
    [JsonProperty("players")]
    public Dictionary<string, Player> Players { get; set; }

    /// <summary>
    /// Encounter
    /// </summary>
    [JsonProperty("encounter")]
    public Encounter Encounter { get; set; }

    /// <summary>
    /// Report
    /// </summary>
    [JsonProperty("report")]
    public Report Report { get; set; }

    /// <summary>
    /// Temp API ID
    /// </summary>
    [JsonProperty("tempApiId")]
    public int TempApiId { get; set; }
}
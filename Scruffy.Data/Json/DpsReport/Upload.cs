using Newtonsoft.Json;

namespace Scruffy.Data.Json.DpsReport;

/// <summary>
/// Upload
/// </summary>
public class Upload
{
    /// <summary>
    /// Id
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Permalink
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

#nullable enable
    /// <summary>
    /// Language
    /// </summary>
    [JsonProperty("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Language id
    /// </summary>
    [JsonProperty("languageId")]
    public int? LanguageId { get; set; }
#nullable disable

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
}
using Newtonsoft.Json;

namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Upload;

/// <summary>
/// Encounter
/// </summary>
public class Encounter
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [JsonProperty("uniqueId")]
    public string UniqueId { get; set; }

    /// <summary>
    /// Sucess
    /// </summary>
    [JsonProperty("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Duration
    /// </summary>
    [JsonProperty("duration")]
    public double Duration { get; set; }

    /// <summary>
    /// Comp DPS
    /// </summary>
    [JsonProperty("compDps")]
    public int CompDps { get; set; }

    /// <summary>
    /// Number of players
    /// </summary>
    [JsonProperty("numberOfPlayers")]
    public int NumberOfPlayers { get; set; }

    /// <summary>
    /// Number of groups
    /// </summary>
    [JsonProperty("numberOfGroups")]
    public int NumberOfGroups { get; set; }

    /// <summary>
    /// Boss ID
    /// </summary>
    [JsonProperty("bossId")]
    public int BossId { get; set; }

    /// <summary>
    /// Boss
    /// </summary>
    [JsonProperty("boss")]
    public string Boss { get; set; }

    /// <summary>
    /// Challenge Mode
    /// </summary>
    [JsonProperty("isCm")]
    public bool IsCm { get; set; }

    /// <summary>
    /// GW2 Build
    /// </summary>
    [JsonProperty("gw2Build")]
    public int Gw2Build { get; set; }

    /// <summary>
    /// Is JSON available?
    /// </summary>
    [JsonProperty("jsonAvailable")]
    public bool JsonAvailable { get; set; }
}
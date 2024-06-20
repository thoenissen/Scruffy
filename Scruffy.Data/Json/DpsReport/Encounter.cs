using Newtonsoft.Json;

using Scruffy.Data.Converter;

namespace Scruffy.Data.Json.DpsReport;

/// <summary>
/// Encounter
/// </summary>
public class Encounter
{
    /// <summary>
    /// Unique id
    /// </summary>
    [JsonProperty("uniqueId")]
    public string UniqueId { get; set; }

    /// <summary>
    /// Success
    /// </summary>
    [JsonProperty("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Duration
    /// </summary>
    [JsonProperty("duration")]
    [JsonConverter(typeof(SecondsTimeSpanConverter))]
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Computed overall DPS of the group.
    /// </summary>
    [JsonProperty("compDps")]
    public int CompDps { get; set; }

    /// <summary>
    /// Number of players
    /// </summary>
    [JsonProperty("numberOfPlayers")]
    public int NumberOfPlayers { get; set; }

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
    /// CM
    /// </summary>
    [JsonProperty("isCm")]
    public bool IsChallengeMode { get; set; }

    /// <summary>
    /// GW2-Build
    /// </summary>
    [JsonProperty("gw2Build")]
    public ulong Gw2Build { get; set; }

    /// <summary>
    /// JSON available
    /// </summary>
    [JsonProperty("jsonAvailable")]
    public bool JsonAvailable { get; set; }
}
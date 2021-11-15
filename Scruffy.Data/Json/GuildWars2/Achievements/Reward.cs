using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Achievements;

/// <summary>
/// Reward
/// </summary>
public class Reward
{
    /// <summary>
    /// Id
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// Type
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// Count
    /// </summary>
    [JsonProperty("count")]
    public int Count { get; set; }

    /// <summary>
    /// Region
    /// </summary>
    [JsonProperty("region")]
    public string Region { get; set; }
}
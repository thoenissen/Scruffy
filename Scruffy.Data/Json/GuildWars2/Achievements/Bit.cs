using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Achievements;

/// <summary>
/// Bit
/// </summary>
public class Bit
{
    /// <summary>
    /// Id
    /// </summary>
    [JsonProperty("id")]
    public int? Id { get; set; }

    /// <summary>
    /// Type
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// Text
    /// </summary>
    [JsonProperty("text")]
    public string Text { get; set; }
}
using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Guild;

/// <summary>
/// Foreground of the emblem
/// </summary>
public class GuildEmblemForeground
{
    /// <summary>
    /// Id
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// An array of numbers containing the id of each color used.
    /// </summary>
    [JsonProperty("colors")]
    public List<int> Colors { get; set; }
}
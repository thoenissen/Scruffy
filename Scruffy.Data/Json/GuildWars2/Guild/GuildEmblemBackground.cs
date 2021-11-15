using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Guild;

/// <summary>
/// Background of the emblem
/// </summary>
public class GuildEmblemBackground
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
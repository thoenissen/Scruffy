using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Guild;

/// <summary>
/// Guild rank
/// </summary>
public class GuildRank
{
    /// <summary>
    /// Id
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Order
    /// </summary>
    [JsonProperty("order")]
    public int Order { get; set; }

    /// <summary>
    /// Permissions
    /// </summary>
    [JsonProperty("permissions")]
    public List<string> Permissions { get; set; }

    /// <summary>
    /// Icon
    /// </summary>
    [JsonProperty("icon")]
    public string Icon { get; set; }
}
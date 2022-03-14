using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Items;

/// <summary>
/// Guild ingredient
/// </summary>
public class GuildIngredient
{
    /// <summary>
    /// Upgrade id
    /// </summary>
    [JsonProperty("upgrade_id")]
    public int UpgradeId { get; set; }

    /// <summary>
    /// Count
    /// </summary>
    [JsonProperty("count")]
    public int Count { get; set; }
}
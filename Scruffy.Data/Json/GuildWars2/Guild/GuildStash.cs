using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Guild;

/// <summary>
/// Guild stash
/// </summary>
public class GuildStash
{
    /// <summary>
    /// Id of the uprgrade
    /// </summary>
    [JsonProperty("upgrade_id")]
    public int UpgradeId { get; set; }

    /// <summary>
    /// Size of the stash
    /// </summary>
    [JsonProperty("size")]
    public int Size { get; set; }

    /// <summary>
    /// Coins
    /// </summary>
    [JsonProperty("coins")]
    public int Coins { get; set; }

    /// <summary>
    /// Note
    /// </summary>
    [JsonProperty("note")]
    public string Note { get; set; }

    /// <summary>
    /// Slots
    /// </summary>
    [JsonProperty("inventory")]
    public List<GuildStashSlot> Slots { get; set; }
}
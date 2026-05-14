using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Items;

/// <summary>
/// Item ingredient
/// </summary>
public class ItemIngredient
{
    #region Properties

    /// <summary>
    /// Item id
    /// </summary>
    [JsonProperty("item_id")]
    public int ItemId { get; set; }

    /// <summary>
    /// Count
    /// </summary>
    [JsonProperty("count")]
    public int Count { get; set; }

    #endregion // Properties
}
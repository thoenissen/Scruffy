using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Items;

/// <summary>
/// Item recipe
/// </summary>
public class ItemRecipe
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
    /// Output item id
    /// </summary>
    [JsonProperty("output_item_id")]
    public int OutputItemId { get; set; }

    /// <summary>
    /// Output item count
    /// </summary>
    [JsonProperty("output_item_count")]
    public int OutputItemCount { get; set; }

    /// <summary>
    /// Time to craft
    /// </summary>
    [JsonProperty("time_to_craft_ms")]
    public int TimeToCraftMs { get; set; }

    /// <summary>
    /// Disciplines
    /// </summary>
    [JsonProperty("disciplines")]
    public List<string> Disciplines { get; set; }

    /// <summary>
    /// Minimal rating
    /// </summary>
    [JsonProperty("min_rating")]
    public int MinRating { get; set; }

    /// <summary>
    /// Flags
    /// </summary>
    [JsonProperty("flags")]
    public List<string> Flags { get; set; }

    /// <summary>
    /// Ingredients
    /// </summary>
    [JsonProperty("ingredients")]
    public List<ItemIngredient> Ingredients { get; set; }

    /// <summary>
    /// Output upgrade id
    /// </summary>
    [JsonProperty("output_upgrade_id")]
    public int OutputUpgradeId { get; set; }

    /// <summary>
    /// Chat link
    /// </summary>
    [JsonProperty("chat_link")]
    public string ChatLink { get; set; }

    /// <summary>
    /// Guild ingredients
    /// </summary>
    [JsonProperty("guild_ingredients")]
    public List<GuildIngredient> GuildIngredients { get; set; }
}
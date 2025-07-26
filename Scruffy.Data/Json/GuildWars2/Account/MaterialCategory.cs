using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Account;

/// <summary>
/// Material category
/// </summary>
public class MaterialCategory
{
    /// <summary>
    /// The category id
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// The category name
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// The ids of the items in this category
    /// </summary>
    [JsonProperty("items")]
    public int[] Items { get; set; }

    /// <summary>
    /// The order in which the category appears in the material storage
    /// </summary>
    [JsonProperty("order")]
    public int Order { get; set; }
}
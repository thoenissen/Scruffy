using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Items;

/// <summary>
/// Item
/// </summary>
public class Item
{
    /// <summary>
    /// Id
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Type
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// Flags
    /// </summary>
    [JsonProperty("Flags")]
    public List<string> Flags { get; set; }

    /// <summary>
    /// Vendor value
    /// </summary>
    [JsonProperty("vendor_value")]
    public long? VendorValue { get; set; }

    /// <summary>
    /// Details
    /// </summary>
    [JsonProperty("details")]
    public ItemDetails Details { get; set; }
}
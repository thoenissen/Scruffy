using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Account;

/// <summary>
/// Account material
/// </summary>
public class AccountMaterial
{
    /// <summary>
    /// The item ID of the material.
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// The material category the item belongs to.
    /// </summary>
    [JsonProperty("category")]
    public int Category { get; set; }

    /// <summary>
    /// The binding of the material. Either Account or omitted.
    /// </summary>
    [JsonProperty("binding")]
    public string Binding { get; set; }

    /// <summary>
    /// The number of the material that is stored in the account vault.
    /// </summary>
    [JsonProperty("count")]
    public int Count { get; set; }
}
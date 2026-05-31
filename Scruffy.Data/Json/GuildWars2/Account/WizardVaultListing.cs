using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Account;

/// <summary>
/// Wizard's Vault listing
/// </summary>
public class WizardVaultListing
{
    #region Properties

    /// <summary>
    /// Listing id
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// Item id
    /// </summary>
    [JsonProperty("item_id")]
    public int ItemId { get; set; }

    /// <summary>
    /// Item count
    /// </summary>
    [JsonProperty("item_count")]
    public int ItemCount { get; set; }

    /// <summary>
    /// Listing type
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// Astral Acclaim cost
    /// </summary>
    [JsonProperty("cost")]
    public int Cost { get; set; }

    /// <summary>
    /// Purchased count
    /// </summary>
    [JsonProperty("purchased")]
    public int? Purchased { get; set; }

    /// <summary>
    /// Purchase limit
    /// </summary>
    [JsonProperty("purchase_limit")]
    public int? PurchaseLimit { get; set; }

    #endregion // Properties
}
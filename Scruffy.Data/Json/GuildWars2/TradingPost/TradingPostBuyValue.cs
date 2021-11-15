using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.TradingPost;

/// <summary>
/// Buy value
/// </summary>
public class TradingPostBuyValue
{
    /// <summary>
    /// Quantity
    /// </summary>
    [JsonProperty("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// Unit proce
    /// </summary>
    [JsonProperty("unit_price")]
    public int UnitPrice { get; set; }
}
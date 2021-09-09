using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.TradingPost
{
    /// <summary>
    /// Sell value
    /// </summary>
    public class TradingPostSellValue
    {
        /// <summary>
        /// Quantity
        /// </summary>
        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// Unit price
        /// </summary>
        [JsonProperty("unit_price")]
        public int UnitPrice { get; set; }
    }
}
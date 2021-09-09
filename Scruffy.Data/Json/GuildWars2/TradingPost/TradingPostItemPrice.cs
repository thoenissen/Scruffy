using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.TradingPost
{
    /// <summary>
    /// Current trading post values
    /// </summary>
    public class TradingPostItemPrice
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Is the item whitelisted?
        /// </summary>
        [JsonProperty("whitelisted")]
        public bool Whitelisted { get; set; }

        /// <summary>
        /// Buy value
        /// </summary>
        [JsonProperty("buys")]
        public TradingPostBuyValue TradingPostBuyValue { get; set; }

        /// <summary>
        /// Sell value
        /// </summary>
        [JsonProperty("sells")]
        public TradingPostSellValue TradingPostSellValue { get; set; }
    }
}
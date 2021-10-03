using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Items
{
    /// <summary>
    /// Details of the item
    /// </summary>
    public class ItemDetails
    {
        /// <summary>
        /// Type
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Guild upgrade id
        /// </summary>
        [JsonProperty("guild_upgrade_id")]
        public int GuildUpgradeId { get; set; }

        /// <summary>
        ///  Unlock type
        /// </summary>
        [JsonProperty("unlock_type")]
        public string UnlockType { get; set; }

        /// <summary>
        ///  Unlock type
        /// </summary>
        [JsonProperty("color_id")]
        public int? ColorId { get; set; }
    }
}
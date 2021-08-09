using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Guild
{
    /// <summary>
    /// Slot in the guild stash
    /// </summary>
    public class GuildStashSlot
    {
        /// <summary>
        /// Id of the item
        /// </summary>
        [JsonProperty("id")]
        public int ItemId { get; set; }

        /// <summary>
        /// Count
        /// </summary>
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}

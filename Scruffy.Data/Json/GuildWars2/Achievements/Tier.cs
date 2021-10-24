using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Achievements
{
    /// <summary>
    /// Tier
    /// </summary>
    public class Tier
    {
        /// <summary>
        /// Count
        /// </summary>
        [JsonProperty("count")]
        public int Count { get; set; }

        /// <summary>
        /// Points
        /// </summary>
        [JsonProperty("points")]
        public int Points { get; set; }
    }
}

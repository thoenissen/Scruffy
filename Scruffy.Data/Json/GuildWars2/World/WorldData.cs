using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.World
{
    /// <summary>
    /// World
    /// </summary>
    public class WorldData
    {
        /// <summary>
        /// id
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Population
        /// </summary>
        [JsonProperty("population")]
        public string Population { get; set; }
    }
}
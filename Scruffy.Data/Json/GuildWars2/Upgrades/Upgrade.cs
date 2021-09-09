using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Upgrades
{
    /// <summary>
    /// Upgrade
    /// </summary>
    public class Upgrade
    {
        /// <summary>
        /// id
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}

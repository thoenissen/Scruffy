using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Items
{
    /// <summary>
    /// Item
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Id
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

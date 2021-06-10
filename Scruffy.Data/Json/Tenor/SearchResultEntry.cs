using Newtonsoft.Json;

namespace Scruffy.Data.Json.Tenor
{
    /// <summary>
    /// Tenor search result entry
    /// </summary>
    public class SearchResultEntry
    {
        /// <summary>
        /// Url
        /// </summary>
        [JsonProperty("itemurl")]
        public string ItemUrl { get; set; }
    }
}

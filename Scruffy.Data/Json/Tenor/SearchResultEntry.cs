using System.Collections.Generic;

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
        [JsonProperty("media")]
        public List<SearchResultMediaEntry> Media { get; set; }

        /// <summary>
        /// Url
        /// </summary>
        [JsonProperty("itemurl")]
        public string ItemUrl { get; set; }
    }

    /// <summary>
    /// Media entry
    /// </summary>
    public class SearchResultMediaEntry
    {
        /// <summary>
        /// Url
        /// </summary>
        [JsonProperty("gif")]
        public MediaData GIF { get; set; }
    }

    /// <summary>
    /// Media data
    /// </summary>
    public class MediaData
    {
        /// <summary>
        /// Url
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}

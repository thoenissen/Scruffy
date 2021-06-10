using System.Collections.Generic;

using Newtonsoft.Json;

namespace Scruffy.Data.Json.Tenor
{
    /// <summary>
    /// Tenor search result container
    /// </summary>
    public class SearchResultRoot
    {
        /// <summary>
        /// Result entries
        /// </summary>
        [JsonProperty("results")]
        public List<SearchResultEntry> Results { get; set; }
    }
}
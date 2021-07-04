using System.Collections.Generic;

using Newtonsoft.Json;

namespace Scruffy.Data.Json.MediaWiki
{
    /// <summary>
    /// Search information
    /// </summary>
    public class SearchQueryQuery
    {
        /// <summary>
        /// Information
        /// </summary>
        [JsonProperty("searchinfo")]
        public SearchQuerySearchInfo SearchInfo { get; set; }

        /// <summary>
        /// Search results
        /// </summary>
        [JsonProperty("search")]
        public List<SearchQuerySearch> Search { get; set; }
    }
}
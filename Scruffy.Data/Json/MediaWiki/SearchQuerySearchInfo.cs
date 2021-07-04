using Newtonsoft.Json;

namespace Scruffy.Data.Json.MediaWiki
{
    /// <summary>
    /// Search info
    /// </summary>
    public class SearchQuerySearchInfo
    {
        /// <summary>
        /// Total hits
        /// </summary>
        [JsonProperty("totalhits")]
        public int TotalHits { get; set; }

        /// <summary>
        /// Suggestion
        /// </summary>
        [JsonProperty("suggestion")]
        public string Suggestion { get; set; }

        /// <summary>
        /// Snippet
        /// </summary>
        [JsonProperty("suggestionsnippet")]
        public string SuggestionSnippet { get; set; }
    }
}
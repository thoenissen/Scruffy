using Newtonsoft.Json;

namespace Scruffy.Data.Json.MediaWiki
{
    /// <summary>
    /// Root
    /// </summary>
    public class SearchQueryRoot
    {
        /// <summary>
        /// Batch completed
        /// </summary>
        [JsonProperty("batchcomplete")]
        public string BatchComplete { get; set; }

        /// <summary>
        /// Continue
        /// </summary>
        [JsonProperty("continue")]
        public SearchQueryContinue Continue { get; set; }

        /// <summary>
        /// Query
        /// </summary>
        [JsonProperty("query")]
        public SearchQueryQuery Query { get; set; }
    }
}
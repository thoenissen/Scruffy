using Newtonsoft.Json;

namespace Scruffy.Data.Json.MediaWiki
{
    /// <summary>
    /// Data to continue the search
    /// </summary>
    public class SearchQueryContinue
    {
        /// <summary>
        /// Offset
        /// </summary>
        [JsonProperty("sroffset")]
        public int SrOffset { get; set; }

        /// <summary>
        /// Continue
        /// </summary>
        [JsonProperty("continue")]
        public string Continue { get; set; }
    }
}

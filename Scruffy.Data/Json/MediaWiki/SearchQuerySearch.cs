using System;

using Newtonsoft.Json;

namespace Scruffy.Data.Json.MediaWiki
{
    /// <summary>
    /// Search Result
    /// </summary>
    public class SearchQuerySearch
    {
        /// <summary>
        /// Ns
        /// </summary>
        [JsonProperty("ns")]
        public int Ns { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Page id
        /// </summary>
        [JsonProperty("pageid")]
        public int PageId { get; set; }

        /// <summary>
        /// Size
        /// </summary>
        [JsonProperty("size")]
        public int Size { get; set; }

        /// <summary>
        /// Word count
        /// </summary>
        [JsonProperty("wordcount")]
        public int WordCount { get; set; }

        /// <summary>
        /// Snippet
        /// </summary>
        [JsonProperty("snippet")]
        public string Snippet { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
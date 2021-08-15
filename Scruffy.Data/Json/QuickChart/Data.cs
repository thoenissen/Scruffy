using System.Collections.Generic;

using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart
{
    /// <summary>
    /// Data
    /// </summary>
    public class Data
    {
        /// <summary>
        /// Labels
        /// </summary>
        [JsonProperty("labels")]
        public List<string> Labels { get; set; }

        /// <summary>
        /// Data sets
        /// </summary>
        [JsonProperty("datasets")]
        public List<DataSet> DataSets { get; set; }
    }
}
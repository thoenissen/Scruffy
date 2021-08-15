using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart
{
    /// <summary>
    /// Annotation
    /// </summary>
    public class Annotation
    {
        /// <summary>
        /// Type
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Mode
        /// </summary>
        [JsonProperty("mode")]
        public string Mode { get; set; }

        /// <summary>
        /// Scale id
        /// </summary>
        [JsonProperty("scaleID")]
        public string ScaleID { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        [JsonProperty("value")]
        public double Value { get; set; }

        /// <summary>
        /// Border color
        /// </summary>
        [JsonProperty("borderColor")]
        public string BorderColor { get; set; }

        /// <summary>
        /// Border width
        /// </summary>
        [JsonProperty("borderWidth")]
        public int BorderWidth { get; set; }
    }
}
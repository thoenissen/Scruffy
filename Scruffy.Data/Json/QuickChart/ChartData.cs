using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart
{
    /// <summary>
    /// Chart data
    /// </summary>
    public class ChartData
    {
        /// <summary>
        /// Width
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; }

        /// <summary>
        /// Height
        /// </summary>
        [JsonProperty("height")]
        public int Height { get; set; }

        /// <summary>
        /// Pixel ratio
        /// </summary>
        [JsonProperty("devicePixelRatio")]
        public double? DevicePixelRatio { get; set; }

        /// <summary>
        /// Background color
        /// </summary>
        [JsonProperty("backgroundColor")]
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Key
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// Chart
        /// </summary>
        [JsonProperty("chart")]
        public string Config { get; set; }

        /// <summary>
        /// Format
        /// </summary>
        [JsonProperty("format")]
        public string Format { get; set; }
    }
}

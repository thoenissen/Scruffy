using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart
{
    /// <summary>
    /// Ticks
    /// </summary>
    public class AxisTicks
    {
        /// <summary>
        /// Font color
        /// </summary>
        [JsonProperty("fontColor")]
        public string FontColor { get; set; }
    }
}
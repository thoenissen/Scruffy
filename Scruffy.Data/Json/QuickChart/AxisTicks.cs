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

    /// <summary>
    /// Ticks
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    public class AxisTicks<T> : AxisTicks where T : struct
    {
        /// <summary>
        /// Minimum value
        /// </summary>
        [JsonProperty("min")]
        public T? MinValue { get; set; }

        /// <summary>
        /// Maximum value
        /// </summary>
        [JsonProperty("max")]
        public T? MaxValue { get; set; }
    }
}
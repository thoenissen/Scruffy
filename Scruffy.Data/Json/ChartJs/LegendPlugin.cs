using System.Text.Json.Serialization;

namespace Scruffy.Data.Json.ChartJs
{
    /// <summary>
    /// Legend plugin configuration
    /// </summary>
    public class LegendPlugin
    {
        /// <summary>
        /// Value indicating whether the legend is displayed
        /// </summary>
        public bool Display { get; set; }

        /// <summary>
        /// Position of the legend (e.g. "top", "left", "bottom", "right")
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Position { get; set; }

        /// <summary>
        /// Labels for the legend
        /// </summary>
        public LegendLabels Labels { get; set; }
    }
}
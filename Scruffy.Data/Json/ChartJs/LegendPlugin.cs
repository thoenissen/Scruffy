namespace Scruffy.Data.Json.ChartJs
{
    /// <summary>
    /// Legend plugin configuration
    /// </summary>
    public class LegendPlugin
    {
        /// <summary>
        /// Value indicating whether the legend is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Labels for the legend
        /// </summary>
        public LegendLabels Labels { get; set; }
    }
}
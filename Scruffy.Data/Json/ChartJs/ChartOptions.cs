namespace Scruffy.Data.Json.ChartJs;

/// <summary>
/// Represents the options for configuring a chart
/// </summary>
public class ChartOptions
{
    /// <summary>
    /// Value indicating whether the chart is responsive
    /// </summary>
    public bool Responsive { get; set; }

    /// <summary>
    /// Value indicating whether the chart should maintain its aspect ratio
    /// </summary>
    public bool MaintainAspectRatio { get; set; }

    /// <summary>
    /// Collection of plugins for the chart
    /// </summary>
    public PluginsCollection Plugins { get; set; }

    /// <summary>
    /// Index axis for the chart (eg, "x" or "y")
    /// </summary>
    public string IndexAxis { get; set; }

    /// <summary>
    /// Configuration for the scales of the chart
    /// </summary>
    public Scales Scales { get; set; }
}
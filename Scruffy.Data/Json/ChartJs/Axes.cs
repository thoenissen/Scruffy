namespace Scruffy.Data.Json.ChartJs;

/// <summary>
/// Represents the configuration for the axes of a chart
/// </summary>
public class Axes
{
    /// <summary>
    /// Configuration for the grid lines of the axis
    /// </summary>
    public GridConfiguration Grid { get; set; }

    /// <summary>
    /// Configuration for the ticks of the axis
    /// </summary>
    public AxisTicks Ticks { get; set; }
}
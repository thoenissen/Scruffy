namespace Scruffy.Data.Json.ChartJs;

/// <summary>
/// Represents a dataset in a chart
/// </summary>
public class DataSet
{
    /// <summary>
    /// Color
    /// </summary>
    public string[] Color { get; set; }

    /// <summary>
    /// Background colors
    /// </summary>
    public string[] BackgroundColor { get; set; }

    /// <summary>
    /// Data points
    /// </summary>
    public double[] Data { get; set; }
}
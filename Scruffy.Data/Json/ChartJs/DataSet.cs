namespace Scruffy.Data.Json.ChartJs;

/// <summary>
/// Represents a dataset in a chart
/// </summary>
public class DataSet
{
    /// <summary>
    /// Background colors for the dataset
    /// </summary>
    public string[] BackgroundColor { get; set; }

    /// <summary>
    /// Data points for the dataset
    /// </summary>
    public double[] Data { get; set; }
}
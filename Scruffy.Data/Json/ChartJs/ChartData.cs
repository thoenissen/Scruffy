namespace Scruffy.Data.Json.ChartJs;

/// <summary>
/// Represents the data for a chart, including datasets and labels
/// </summary>
public class ChartData
{
    /// <summary>
    /// Datasets used in the chart
    /// </summary>
    public DataSet[] Datasets { get; set; }

    /// <summary>
    /// Labels for the chart
    /// </summary>
    public string[] Labels { get; set; }
}
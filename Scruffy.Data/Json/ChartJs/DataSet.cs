using System.Text.Json.Serialization;

namespace Scruffy.Data.Json.ChartJs;

/// <summary>
/// Represents a dataset in a chart
/// </summary>
public class DataSet
{
    /// <summary>
    /// Label
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Label { get; set; }

    /// <summary>
    /// Chart type override for mixed charts (e.g. "line" dataset on a "bar" chart)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Type { get; set; }

    /// <summary>
    /// Color
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] Color { get; set; }

    /// <summary>
    /// Background colors
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] BackgroundColor { get; set; }

    /// <summary>
    /// Border colors
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] BorderColor { get; set; }

    /// <summary>
    /// Border width
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? BorderWidth { get; set; }

    /// <summary>
    /// Point radius (for line datasets)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PointRadius { get; set; }

    /// <summary>
    /// Data points
    /// </summary>
    public double[] Data { get; set; }
}
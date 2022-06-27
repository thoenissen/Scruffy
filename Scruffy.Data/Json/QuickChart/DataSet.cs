using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart;

/// <summary>
/// Data set
/// </summary>
public class DataSet
{
    /// <summary>
    /// Label
    /// </summary>
    [JsonProperty("label")]
    public string Label { get; set; }

    /// <summary>
    /// Fill
    /// </summary>
    [JsonProperty("fill")]
    public bool Fill { get; set; }

    /// <summary>
    /// Border color
    /// </summary>
    [JsonProperty("borderColor")]
    public string BorderColor { get; set; }

    /// <summary>
    /// Background color
    /// </summary>
    [JsonProperty("backgroundColor")]
    public List<string> BackgroundColor { get; set; }
}

/// <summary>
/// Data set
/// </summary>
/// <typeparam name="T">Type</typeparam>
public class DataSet<T> : DataSet
{
    /// <summary>
    /// Data
    /// </summary>
    [JsonProperty("data")]
    public List<T> Data { get; set; }

    /// <summary>
    /// Point radius
    /// </summary>
    [JsonProperty("pointRadius")]
    public double? PointRadius { get; set; }

    /// <summary>
    /// Border dash
    /// </summary>
    [JsonProperty("borderDash")]
    public double[] BorderDash { get; set; }
}
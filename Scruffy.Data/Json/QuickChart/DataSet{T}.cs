using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart;

/// <summary>
/// Data set
/// </summary>
/// <typeparam name="T">Type</typeparam>
public class DataSet<T> : DataSet
{
    #region Properties

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

    #endregion // Properties
}
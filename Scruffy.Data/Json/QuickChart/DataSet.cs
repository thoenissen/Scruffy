using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart;

/// <summary>
/// Data set
/// </summary>
public class DataSet
{
    #region Properties

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

    #endregion // Properties
}
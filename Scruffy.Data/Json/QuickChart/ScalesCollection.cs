using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart;

/// <summary>
/// Scales collections
/// </summary>
public class ScalesCollection
{
    #region Properties

    /// <summary>
    /// X-Axes
    /// </summary>
    [JsonProperty("yAxes")]
    public List<YAxis> YAxes { get; set; }

    /// <summary>
    /// X-Axes
    /// </summary>
    [JsonProperty("xAxes")]
    public List<XAxis> XAxes { get; set; }

    #endregion // Properties
}
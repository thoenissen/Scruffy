using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart;

/// <summary>
/// Y-Axis
/// </summary>
public class YAxis
{
    #region Properties

    /// <summary>
    /// Stacked
    /// </summary>
    [JsonProperty("stacked")]
    public bool? Stacked { get; set; }

    /// <summary>
    /// Ticks
    /// </summary>
    [JsonProperty("ticks")]
    public AxisTicks Ticks { get; set; }

    #endregion // Properties
}
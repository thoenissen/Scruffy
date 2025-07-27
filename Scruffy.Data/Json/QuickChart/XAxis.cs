using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart;

/// <summary>
/// X-Axis
/// </summary>
public class XAxis
{
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
}
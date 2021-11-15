using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart;

/// <summary>
/// Y-Axis
/// </summary>
public class YAxis
{
    /// <summary>
    /// Ticks
    /// </summary>
    [JsonProperty("ticks")]
    public AxisTicks Ticks { get; set; }
}
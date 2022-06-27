using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart;

/// <summary>
/// Chart legend configuration
/// </summary>
public class ChartLegendConfiguration
{
    /// <summary>
    /// Position
    /// </summary>
    [JsonProperty("position")]
    public string Position { get; set; }
}
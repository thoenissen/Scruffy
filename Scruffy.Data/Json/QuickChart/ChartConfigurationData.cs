using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart;

/// <summary>
/// Chart configuration
/// </summary>
public class ChartConfigurationData
{
    #region Properties

    /// <summary>
    /// Type
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// Data
    /// </summary>
    [JsonProperty("data")]
    public Data Data { get; set; }

    /// <summary>
    /// Options
    /// </summary>
    [JsonProperty("options")]
    public OptionsCollection Options { get; set; }

    #endregion // Properties
}
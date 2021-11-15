using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart;

/// <summary>
/// Plugins
/// </summary>
public class PluginsCollection
{
    /// <summary>
    /// Legend
    /// </summary>
    [JsonProperty("legend")]
    public bool Legend { get; set; }

    /// <summary>
    /// Out labels
    /// </summary>
    [JsonProperty("outlabels")]
    public OutLabelsCollection OutLabels { get; set; }

    /// <summary>
    /// Doughnut label
    /// </summary>
    [JsonProperty("doughnutlabel")]
    public DoughnutLabelCollection DoughnutLabel { get; set; }
}
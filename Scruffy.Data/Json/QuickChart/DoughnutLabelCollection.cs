using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart;

/// <summary>
/// Doughnut labels
/// </summary>
public class DoughnutLabelCollection
{
    #region Properties

    /// <summary>
    /// Doughnut label
    /// </summary>
    [JsonProperty("labels")]
    public List<Label> Labels { get; set; }

    #endregion // Properties
}
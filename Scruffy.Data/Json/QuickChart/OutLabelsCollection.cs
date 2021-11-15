using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart;

/// <summary>
/// Out labels
/// </summary>
public class OutLabelsCollection
{
    /// <summary>
    /// Text
    /// </summary>
    [JsonProperty("text")]
    public string Text { get; set; }

    /// <summary>
    /// Stretch
    /// </summary>
    [JsonProperty("stretch")]
    public int Stretch { get; set; }
}
using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart;

/// <summary>
/// Title
/// </summary>
public class TitleConfiguration
{
    /// <summary>
    /// Display
    /// </summary>
    [JsonProperty("display")]
    public bool Display { get; set; }

    /// <summary>
    /// Text
    /// </summary>
    [JsonProperty("text")]
    public string Text { get; set; }

    /// <summary>
    /// Font size
    /// </summary>
    [JsonProperty("fontColor")]
    public string FontColor { get; set; }

    /// <summary>
    /// Font size
    /// </summary>
    [JsonProperty("fontSize")]
    public int FontSize { get; set; }
}
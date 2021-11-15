using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart;

/// <summary>
/// Options
/// </summary>
public class OptionsCollection
{
    /// <summary>
    /// Annotations
    /// </summary>
    [JsonProperty("annotation")]
    public AnnotationsCollection Annotation { get; set; }

    /// <summary>
    /// Scales
    /// </summary>
    [JsonProperty("scales")]
    public ScalesCollection Scales { get; set; }

    /// <summary>
    /// Plugins
    /// </summary>
    [JsonProperty("plugins")]
    public PluginsCollection Plugins { get; set; }

    /// <summary>
    /// Title
    /// </summary>
    [JsonProperty("title")]
    public TitleConfiguration Title { get; set; }
}
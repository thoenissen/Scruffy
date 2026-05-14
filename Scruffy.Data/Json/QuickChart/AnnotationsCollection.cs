using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart;

/// <summary>
/// Collection of annotations
/// </summary>
public class AnnotationsCollection
{
    #region Properties

    /// <summary>
    /// Annotations
    /// </summary>
    [JsonProperty("annotations")]
    public List<Annotation> Annotations { get; set; }

    #endregion // Properties
}
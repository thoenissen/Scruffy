using Newtonsoft.Json;

namespace Scruffy.Data.Json.Tenor;

/// <summary>
/// Media data
/// </summary>
public class MediaData
{
    #region Properties

    /// <summary>
    /// Url
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; }

    /// <summary>
    /// Size
    /// </summary>
    [JsonProperty("size")]
    public int Size { get; set; }

    #endregion // Properties
}
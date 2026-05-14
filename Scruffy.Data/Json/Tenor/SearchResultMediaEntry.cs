using Newtonsoft.Json;

namespace Scruffy.Data.Json.Tenor;

/// <summary>
/// Media entry
/// </summary>
public class SearchResultMediaEntry
{
    #region Properties

    /// <summary>
    /// Url
    /// </summary>
    [JsonProperty("nanogif")]
    public MediaData NanoGif { get; set; }

    /// <summary>
    /// Url
    /// </summary>
    [JsonProperty("mediumgif")]
    public MediaData MediumGif { get; set; }

    /// <summary>
    /// Url
    /// </summary>
    [JsonProperty("gif")]
    public MediaData Gif { get; set; }

    #endregion // Properties
}
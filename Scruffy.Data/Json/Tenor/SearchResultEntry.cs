using Newtonsoft.Json;

namespace Scruffy.Data.Json.Tenor;

/// <summary>
/// Tenor search result entry
/// </summary>
public class SearchResultEntry
{
    #region Properties

    /// <summary>
    /// Url
    /// </summary>
    [JsonProperty("media")]
    public List<SearchResultMediaEntry> Media { get; set; }

    /// <summary>
    /// Url
    /// </summary>
    [JsonProperty("itemurl")]
    public string ItemUrl { get; set; }

    #endregion // Properties
}
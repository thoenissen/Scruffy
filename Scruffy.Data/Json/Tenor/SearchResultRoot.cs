using Newtonsoft.Json;

namespace Scruffy.Data.Json.Tenor;

/// <summary>
/// Tenor search result container
/// </summary>
public class SearchResultRoot
{
    #region Properties

    /// <summary>
    /// Result entries
    /// </summary>
    [JsonProperty("results")]
    public List<SearchResultEntry> Results { get; set; }

    #endregion // Properties
}
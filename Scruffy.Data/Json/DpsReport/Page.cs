using Newtonsoft.Json;

namespace Scruffy.Data.Json.DpsReport;

/// <summary>
/// Pages
/// </summary>
public class Page
{
    #region Properties

    /// <summary>
    /// Pages
    /// </summary>
    [JsonProperty("pages")]
    public int Pages { get; set; }

    /// <summary>
    /// Uploads
    /// </summary>
    [JsonProperty("uploads")]
    public List<Upload> Uploads { get; set; }

    #endregion // Properties
}
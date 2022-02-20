using Newtonsoft.Json;

namespace Scruffy.Data.Json.DpsReport;

/// <summary>
/// Pages
/// </summary>
public class Page
{
    /// <summary>
    /// Pages
    /// </summary>
    [JsonProperty("pages")]
    public int Pages { get; set; }

    /// <summary>
    /// Total uploads
    /// </summary>
    [JsonProperty("totalUploads")]
    public string TotalUploads { get; set; }

    /// <summary>
    /// User token
    /// </summary>
    [JsonProperty("userToken")]
    public string UserToken { get; set; }

    /// <summary>
    /// Uploads
    /// </summary>
    [JsonProperty("uploads")]
    public List<Upload> Uploads { get; set; }
}
using Newtonsoft.Json;

namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Upload;

/// <summary>
/// Page
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
    /// User tokens
    /// </summary>
    [JsonProperty("userToken")]
    public string UserToken { get; set; }

    /// <summary>
    /// Uploads
    /// </summary>
    [JsonProperty("uploads")]
    public List<MetaData> Uploads { get; set; }
}
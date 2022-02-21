using Newtonsoft.Json;

namespace Scruffy.Data.Json.GitHub;

/// <summary>
/// Git Author
/// </summary>
public class GitAuthor
{
    /// <summary>
    /// Date
    /// </summary>
    [JsonProperty("date")]
    public DateTime Date { get; set; }
}
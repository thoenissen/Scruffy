using Newtonsoft.Json;

namespace Scruffy.Data.Json.GitHub;

/// <summary>
/// Git Committer
/// </summary>
public class GitCommitter
{
    /// <summary>
    /// Date
    /// </summary>
    [JsonProperty("date")]
    public DateTime Date { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }
}
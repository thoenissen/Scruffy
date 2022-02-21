using Newtonsoft.Json;

namespace Scruffy.Data.Json.GitHub;

/// <summary>
/// GitHub commit
/// </summary>
public class GitHubCommit
{
    /// <summary>
    /// SHA-Checksum
    /// </summary>
    [JsonProperty("sha")]
    public string Sha { get; set; }

    /// <summary>
    /// Commit data
    /// </summary>
    [JsonProperty("commit")]
    public GitCommit GitCommit { get; set; }

    /// <summary>
    /// Author
    /// </summary>
    [JsonProperty("author")]
    public GitHubAccountAuthor Author { get; set; }

    /// <summary>
    /// Committer
    /// </summary>
    [JsonProperty("committer")]
    public GitHubAccountCommitter Committer { get; set; }
}
using Newtonsoft.Json;

namespace Scruffy.Data.Json.GitHub;

/// <summary>
/// Data of a commit
/// </summary>
public class GitCommit
{
    /// <summary>
    /// Author
    /// </summary>
    [JsonProperty("author")]
    public GitAuthor Author { get; set; }

    /// <summary>
    /// Committer
    /// </summary>
    [JsonProperty("committer")]
    public GitCommitter Committer { get; set; }
}
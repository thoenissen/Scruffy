using Newtonsoft.Json;

namespace Scruffy.Data.Json.GitHub;

/// <summary>
/// GitHub branch
/// </summary>
public class GitHubBranch
{
    /// <summary>
    /// Name
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Commit
    /// </summary>
    [JsonProperty("commit")]
    public GitHubBranchCommit Commit { get; set; }
}
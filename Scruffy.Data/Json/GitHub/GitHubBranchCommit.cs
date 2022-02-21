using Newtonsoft.Json;

namespace Scruffy.Data.Json.GitHub;

/// <summary>
/// GitHub branch commit
/// </summary>
public class GitHubBranchCommit
{
    /// <summary>
    /// SHA-Checksum
    /// </summary>
    [JsonProperty("sha")]
    public string Sha { get; set; }
}
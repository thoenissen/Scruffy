using Newtonsoft.Json;

namespace Scruffy.Data.Json.GitHub;

/// <summary>
/// GitHub-Account Committer
/// </summary>
public class GitHubAccountCommitter
{
    #region Properties

    /// <summary>
    /// Login
    /// </summary>
    [JsonProperty("login")]
    public string Login { get; set; }

    #endregion // Properties
}
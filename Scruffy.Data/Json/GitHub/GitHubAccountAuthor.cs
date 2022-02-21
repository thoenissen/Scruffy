using Newtonsoft.Json;

namespace Scruffy.Data.Json.GitHub;

/// <summary>
/// Author
/// </summary>
public class GitHubAccountAuthor
{
    /// <summary>
    /// Login
    /// </summary>
    [JsonProperty("login")]
    public string Login { get; set; }
}
using System.Net.Http;

using Newtonsoft.Json;

using Scruffy.Data.Json.GitHub;

namespace Scruffy.Services.WebApi;

/// <summary>
/// GitHub connector
/// </summary>
public class GitHubConnector
{
    #region Fields

    /// <summary>
    /// Client factory
    /// </summary>
    private readonly IHttpClientFactory _clientFactory;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="clientFactory">Client factory</param>
    public GitHubConnector(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Get branches
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="repository">Repository</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<List<GitHubBranch>> GetBranches(string user, string repository)
    {
        using (var response = await _clientFactory.CreateClient("GitHub")
                                                  .GetAsync($"https://api.github.com/repos/{user}/{repository}/branches")
                                                  .ConfigureAwait(false))
        {
            var jsonResult = await response.Content
                                           .ReadAsStringAsync()
                                           .ConfigureAwait(false);

            return JsonConvert.DeserializeObject<List<GitHubBranch>>(jsonResult);
        }
    }

    /// <summary>
    /// Get commits
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="repository">Repository</param>
    /// <param name="startCommitSha">SHA of first commit</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<List<GitHubCommit>> GetCommits(string user, string repository, string startCommitSha)
    {
        using (var response = await _clientFactory.CreateClient("GitHub")
                                                  .GetAsync($"https://api.github.com/repos/{user}/{repository}/commits?per_page=100&sha={startCommitSha}")
                                                  .ConfigureAwait(false))
        {
            var jsonResult = await response.Content
                                           .ReadAsStringAsync()
                                           .ConfigureAwait(false);

            return JsonConvert.DeserializeObject<List<GitHubCommit>>(jsonResult);
        }
    }

    #endregion // Methods
}
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Developer;
using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Developer;

/// <summary>
/// Developer service
/// </summary>
public class DeveloperService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory;

    /// <summary>
    /// GitHub connector
    /// </summary>
    private readonly GitHubConnector _gitHubConnector;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="repositoryFactory">Repository factory</param>
    /// <param name="gitHubConnector">GitHub connector</param>
    public DeveloperService(LocalizationService localizationService,
                            RepositoryFactory repositoryFactory,
                            GitHubConnector gitHubConnector)
        : base(localizationService)
    {
        _repositoryFactory = repositoryFactory;
        _gitHubConnector = gitHubConnector;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Import commits
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ImportCommits()
    {
        try
        {
            var commits = new Dictionary<string, (string Sha, string Author, string Committer, DateTime TimeStamp)>();

            var branches = await _gitHubConnector.GetBranches("thoenissen", "scruffy")
                                                 .ConfigureAwait(false);

            var main = branches.FirstOrDefault(obj => obj.Name == "main");

            if (main != null)
            {
                var commitSha = main.Commit?.Sha;

                while (commitSha != null)
                {
                    var currentCommits = await _gitHubConnector.GetCommits("thoenissen", "scruffy", commitSha)
                                                               .ConfigureAwait(false);

                    foreach (var commit in currentCommits)
                    {
                        var author = commit.Author?.Login ?? commit.GitCommit.Author.Name;

                        if (author == "web-flow")
                        {
                            author = "thoenissen";
                        }

                        var committer = commit.Committer?.Login ?? commit.GitCommit.Committer.Name;

                        if (committer == "web-flow")
                        {
                            committer = "thoenissen";
                        }

                        commits[commit.Sha] = (commit.Sha, author, committer, commit.GitCommit.Committer.Date.ToLocalTime());
                    }

                    var lastSha = currentCommits.LastOrDefault()?.Sha;

                    commitSha = lastSha != commitSha
                                    ? lastSha
                                    : null;
                }
            }

            if (await _repositoryFactory.GetRepository<GitHubCommitRepository>()
                                        .BulkInsert(commits.Values)
                                        .ConfigureAwait(false) == false)
            {
                LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(DeveloperService), "Bulk insert failed", null, _repositoryFactory.LastError);
            }
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(DeveloperService), "GitHub api call failed", null, ex);
        }
    }

    #endregion // Methods
}
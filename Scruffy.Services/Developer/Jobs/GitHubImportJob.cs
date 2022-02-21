using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Developer.Jobs
{
    /// <summary>
    /// GitHub import job
    /// </summary>
    public class GitHubImportJob : LocatedAsyncJob
    {
        #region Fields

        /// <summary>
        /// Developer service
        /// </summary>
        private readonly DeveloperService _developerService;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="developerService">Developer service</param>
        public GitHubImportJob(DeveloperService developerService)
        {
            _developerService = developerService;
        }

        #endregion // Constructor

        #region LocatedAsyncJob

        /// <summary>
        /// Executes the job
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task ExecuteOverrideAsync()
        {
            await _developerService.ImportCommits()
                                   .ConfigureAwait(false);
        }

        #endregion // LocatedAsyncJob
    }
}

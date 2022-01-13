using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Guild.Jobs
{
    /// <summary>
    /// Daily point refresh
    /// </summary>
    public class GuildRankCurrentPointsJob : LocatedAsyncJob
    {
        #region LocatedAsyncJob

        /// <summary>
        /// Executes the job
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task ExecuteAsync()
        {
            var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider();
            await using (serviceProvider.ConfigureAwait(false))
            {
                await serviceProvider.GetService<GuildRankService>()
                                     .RefreshCurrentPoints(null)
                                     .ConfigureAwait(false);
            }
        }

        #endregion // LocatedAsyncJob
    }
}

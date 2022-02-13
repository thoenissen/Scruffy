using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Guild.Jobs;

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
    public override async Task ExecuteOverrideAsync()
    {
        var serviceProvider = Core.ServiceProviderContainer.Current.GetServiceProvider();
        await using (serviceProvider.ConfigureAwait(false))
        {
            await serviceProvider.GetRequiredService<GuildRankService>()
                                 .RefreshCurrentPoints(null)
                                 .ConfigureAwait(false);
        }
    }

    #endregion // LocatedAsyncJob
}
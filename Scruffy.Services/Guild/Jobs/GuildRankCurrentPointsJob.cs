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

    /// <inheritdoc/>
    public override async Task ExecuteOverrideAsync()
    {
        var serviceProvider = ServiceProviderContainer.Current.GetServiceProvider();

        await using (serviceProvider.ConfigureAwait(false))
        {
            await serviceProvider.GetRequiredService<GuildRankService>()
                                 .RefreshCurrentPoints(null)
                                 .ConfigureAwait(false);
        }
    }

    #endregion // LocatedAsyncJob
}
using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Guild.Jobs;

/// <summary>
/// Guild member rank import
/// </summary>
public class GuildRankImportJob : LocatedAsyncJob
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
            var guildRankService = serviceProvider.GetService<GuildRankService>();

            await guildRankService.ImportGuildRanks(null)
                                  .ConfigureAwait(false);

            await guildRankService.RefreshDiscordRoles(null)
                                  .ConfigureAwait(false);
        }
    }

    #endregion // LocatedAsyncJob
}
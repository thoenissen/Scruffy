using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Exceptions.WebApi;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.GuildWars2.Jobs;

/// <summary>
/// Importing achievements
/// </summary>
public class AchievementImportJob : LocatedAsyncJob
{
    #region LocatedAsyncJob

    /// <inheritdoc/>
    public override async Task ExecuteOverrideAsync()
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var serviceProvider = ServiceProviderContainer.Current.GetServiceProvider();

            await using (serviceProvider.ConfigureAwait(false))
            {
                var achievementService = serviceProvider.GetService<AchievementService>();

                await achievementService.ImportAchievements()
                                        .ConfigureAwait(false);

                foreach (var account in await dbFactory.GetRepository<GuildWarsAccountRepository>()
                                                       .GetQuery()
                                                       .Where(obj => obj.Permissions.HasFlag(GuildWars2ApiPermission.RequiredPermissions))
                                                       .Select(obj => new
                                                                      {
                                                                          obj.Name,
                                                                          obj.ApiKey
                                                                      })
                                                       .ToListAsync()
                                                       .ConfigureAwait(false))
                {
                    try
                    {
                        await achievementService.ImportAccountAchievements(account.Name, account.ApiKey)
                                                .ConfigureAwait(false);
                    }
                    catch (MissingGuildWars2ApiPermissionException ex)
                    {
                        LoggingService.AddJobLogEntry(LogEntryLevel.Error, nameof(AchievementImportJob), $"Missing permissions {account}", null, ex);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.AddJobLogEntry(LogEntryLevel.Error, nameof(AchievementImportJob), $"Unknown error with account {account}", null, ex);
                    }
                }

                await dbFactory.GetRepository<GuildWarsAccountRankingDataRepository>()
                               .InsertCurrentAchievementPoints()
                               .ConfigureAwait(false);
            }
        }
    }

    #endregion // LocatedAsyncJob
}
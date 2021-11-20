using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.GuildWars2.Jobs
{
    /// <summary>
    /// Importing achievements
    /// </summary>
    public class AchievementImportJob : LocatedAsyncJob
    {
        #region LocatedAsyncJob

        /// <summary>
        /// Executes the job
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task ExecuteAsync()
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider();
                await using (serviceProvider.ConfigureAwait(false))
                {
                    var achievementService = serviceProvider.GetService<AchievementService>();

                    await achievementService.ImportAchievements()
                                            .ConfigureAwait(false);

                    foreach (var account in await dbFactory.GetRepository<GuildWarsAccountRepository>()
                                                           .GetQuery()
                                                           .Select(obj => new
                                                                          {
                                                                              obj.Name,
                                                                              obj.ApiKey
                                                                          })
                                                           .ToListAsync()
                                                           .ConfigureAwait(false))
                    {
                        await achievementService.ImportAccountAchievements(account.Name, account.ApiKey)
                                                .ConfigureAwait(false);
                    }
                }
            }
        }

        #endregion // LocatedAsyncJob
    }
}

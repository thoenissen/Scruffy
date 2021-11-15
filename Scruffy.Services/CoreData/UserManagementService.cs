using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.CoreData;
using Scruffy.Data.Entity.Tables.Discord;
using Scruffy.Data.Enumerations.CoreData;
using Scruffy.Data.Services.CoreData;

namespace Scruffy.Services.CoreData;

/// <summary>
/// Management of users
/// </summary>
public class UserManagementService
{
    #region Methods

    /// <summary>
    /// Checks the given user and creates a new entry of the user doesn't exists
    /// </summary>
    /// <param name="discordUserId">Id of the discord user</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CheckDiscordAccountAsync(ulong discordUserId)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            if (await dbFactory.GetRepository<DiscordAccountRepository>()
                               .GetQuery()
                               .AnyAsync(obj => obj.Id == discordUserId)
                               .ConfigureAwait(false) == false)
            {
                var user = new UserEntity
                           {
                               CreationTimeStamp = DateTime.Now,
                               Type = UserType.DiscordUser
                           };

                if (dbFactory.GetRepository<UserRepository>()
                             .Add(user))
                {
                    dbFactory.GetRepository<DiscordAccountRepository>()
                             .Add(new DiscordAccountEntity
                                  {
                                      Id = discordUserId,
                                      UserId = user.Id
                                  });
                }
            }
        }
    }

    /// <summary>
    /// Get user raid experience rank
    /// </summary>
    /// <param name="discordAccountId">Id of the discord account</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<int> GetRaidExperienceLevelRankByDiscordUserId(ulong discordAccountId)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var rank = await dbFactory.GetRepository<DiscordAccountRepository>()
                                      .GetQuery()
                                      .Where(obj => obj.Id == discordAccountId)
                                      .Select(obj => obj.User.RaidExperienceLevel != null ? obj.User.RaidExperienceLevel.Rank : 0)
                                      .FirstOrDefaultAsync()
                                      .ConfigureAwait(false);

            if (rank == 0)
            {
                var defaultRank = await dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                 .GetQuery()
                                                 .OrderByDescending(obj => obj.Rank)
                                                 .FirstOrDefaultAsync()
                                                 .ConfigureAwait(false);

                rank = defaultRank.Rank;

                if (dbFactory.GetRepository<UserRepository>()
                             .Refresh(obj => obj.DiscordAccounts.Any(obj2 => obj2.Id == discordAccountId),
                                      obj => obj.RaidExperienceLevelId = defaultRank.Id) == false)
                {
                    var user = new UserEntity
                               {
                                   CreationTimeStamp = DateTime.Now,
                                   Type = UserType.DiscordUser,
                                   RaidExperienceLevelId = defaultRank.Id
                               };

                    if (dbFactory.GetRepository<UserRepository>()
                                 .Add(user))
                    {
                        dbFactory.GetRepository<DiscordAccountRepository>()
                                 .Add(new DiscordAccountEntity
                                      {
                                          Id = discordAccountId,
                                          UserId = user.Id
                                      });
                    }
                }
            }

            return rank;
        }
    }

    /// <summary>
    /// Get user data
    /// </summary>
    /// <param name="discordAccountId">Id of the discord account</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<UserData> GetUserByDiscordAccountId(ulong discordAccountId)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var userData =  await dbFactory.GetRepository<DiscordAccountRepository>()
                                           .GetQuery()
                                           .Where(obj => obj.Id == discordAccountId)
                                           .Select(obj => new UserData
                                                          {
                                                              Id = obj.UserId,
                                                              ExperienceLevelRank = obj.User.RaidExperienceLevel != null ? obj.User.RaidExperienceLevel.Rank : 0
                                                          })
                                           .FirstOrDefaultAsync()
                                           .ConfigureAwait(false);

            if (userData?.ExperienceLevelRank == 0)
            {
                userData.ExperienceLevelRank = await GetRaidExperienceLevelRankByDiscordUserId(discordAccountId).ConfigureAwait(false);
            }

            return userData;
        }
    }

    #endregion // Methods
}
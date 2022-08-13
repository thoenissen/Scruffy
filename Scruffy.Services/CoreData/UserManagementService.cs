using Discord;

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
    /// <param name="discordUser">Discord user</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CheckDiscordAccountAsync(IUser discordUser)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            if (await dbFactory.GetRepository<DiscordAccountRepository>()
                               .GetQuery()
                               .AnyAsync(obj => obj.Id == discordUser.Id)
                               .ConfigureAwait(false) == false)
            {
                var user = new UserEntity
                           {
                               CreationTimeStamp = DateTime.Now,
                               Type = UserType.DiscordUser,
                               UserName = $"{discordUser.Username}#{discordUser.Discriminator}",
                               SecurityStamp = Guid.NewGuid().ToString()
                           };

                if (dbFactory.GetRepository<UserRepository>()
                             .Add(user))
                {
                    dbFactory.GetRepository<DiscordAccountRepository>()
                             .Add(new DiscordAccountEntity
                                  {
                                      Id = discordUser.Id,
                                      UserId = user.Id
                                  });
                }
            }
        }
    }

    /// <summary>
    /// Get user raid experience rank
    /// </summary>
    /// <param name="discordUser">Discord user</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<int> GetRaidExperienceLevelRankByDiscordUserId(IUser discordUser)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var rank = await dbFactory.GetRepository<DiscordAccountRepository>()
                                      .GetQuery()
                                      .Where(obj => obj.Id == discordUser.Id)
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
                             .Refresh(obj => obj.DiscordAccounts.Any(obj2 => obj2.Id == discordUser.Id),
                                      obj => obj.RaidExperienceLevelId = defaultRank.Id) == false)
                {
                    var user = new UserEntity
                               {
                                   CreationTimeStamp = DateTime.Now,
                                   Type = UserType.DiscordUser,
                                   RaidExperienceLevelId = defaultRank.Id,
                                   UserName = $"{discordUser.Username}#{discordUser.Discriminator}",
                                   SecurityStamp = Guid.NewGuid().ToString()
                               };

                    if (dbFactory.GetRepository<UserRepository>()
                                 .Add(user))
                    {
                        dbFactory.GetRepository<DiscordAccountRepository>()
                                 .Add(new DiscordAccountEntity
                                      {
                                          Id = discordUser.Id,
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
    /// <param name="discordUser">Discord user</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<UserData> GetUserByDiscordAccountId(IUser discordUser)
    {
        await CheckDiscordAccountAsync(discordUser).ConfigureAwait(false);

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var userData =  await dbFactory.GetRepository<DiscordAccountRepository>()
                                           .GetQuery()
                                           .Where(obj => obj.Id == discordUser.Id)
                                           .Select(obj => new UserData
                                                          {
                                                              Id = obj.UserId,
                                                              ExperienceLevelRank = obj.User.RaidExperienceLevel != null
                                                                                        ? obj.User.RaidExperienceLevel.Rank
                                                                                        : 0,
                                                              IsDataStorageAccepted = obj.User.IsDataStorageAccepted
                                                          })
                                           .FirstAsync()
                                           .ConfigureAwait(false);

            if (userData.ExperienceLevelRank == 0)
            {
                userData.ExperienceLevelRank = await GetRaidExperienceLevelRankByDiscordUserId(discordUser).ConfigureAwait(false);
            }

            return userData;
        }
    }

    /// <summary>
    /// Refresh <see cref="UserEntity.IsDataStorageAccepted"/>
    /// </summary>
    /// <param name="userDataId">User id</param>
    /// <param name="isAccepted">Is accepted?</param>
    /// <returns>Successful refresh?</returns>
    public bool SetIsDataStorageAccepted(long userDataId, bool isAccepted)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            return dbFactory.GetRepository<UserRepository>()
                            .Refresh(obj => obj.Id == userDataId, obj => obj.IsDataStorageAccepted = isAccepted);
        }
    }

    #endregion // Methods
}
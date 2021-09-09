using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Services.CoreData
{
    /// <summary>
    /// Management of users
    /// </summary>
    public class UserManagementService
    {
        #region Methods

        /// <summary>
        /// Checks the given user and creates a new entry of the user doesn't exists
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CheckUserAsync(ulong userId)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var userRepository = dbFactory.GetRepository<UserRepository>();
                if (await userRepository.GetQuery().AnyAsync(obj => obj.Id == userId).ConfigureAwait(false) == false)
                {
                    userRepository.Add(new UserEntity
                                       {
                                           Id = userId,
                                           CreationTimeStamp = DateTime.Now
                                       });
                }
            }
        }

        /// <summary>
        /// Get user raid experience rank
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<int> GetRaidExperienceLevelRank(ulong userId)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var rank = await dbFactory.GetRepository<UserRepository>()
                                          .GetQuery()
                                          .Where(obj => obj.Id == userId)
                                          .Select(obj => obj.RaidExperienceLevel.Rank)
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

                    dbFactory.GetRepository<UserRepository>()
                             .AddOrRefresh(obj => obj.Id == userId,
                                           obj =>
                                           {
                                               if (obj.Id == 0)
                                               {
                                                   obj.Id = userId;
                                                   obj.CreationTimeStamp = DateTime.Now;
                                               }

                                               obj.RaidExperienceLevelId = defaultRank.Id;
                                           });
                }

                return rank;
            }
        }

        #endregion // Methods
    }
}

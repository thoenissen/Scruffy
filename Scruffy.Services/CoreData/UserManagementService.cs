using System;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
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
                                           CreationTimeStamp = DateTime.Now.ToUniversalTime()
                                       });
                }
            }
        }

        #endregion // Methods
    }
}

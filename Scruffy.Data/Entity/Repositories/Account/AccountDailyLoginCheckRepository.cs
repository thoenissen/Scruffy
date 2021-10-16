using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Account;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.Account;

namespace Scruffy.Data.Entity.Repositories.Account
{
    /// <summary>
    /// Repository for accessing <see cref="GuildWarsAccountDailyLoginCheckEntity"/>
    /// </summary>
    public class AccountDailyLoginCheckRepository : RepositoryBase<AccountDailyLoginCheckQueryable, GuildWarsAccountDailyLoginCheckEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public AccountDailyLoginCheckRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}

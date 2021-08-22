using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Account;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Account;

namespace Scruffy.Data.Entity.Repositories.Account
{
    /// <summary>
    /// Repository for accessing <see cref="AccountDailyLoginCheckEntity"/>
    /// </summary>
    public class AccountDailyLoginCheckRepository : RepositoryBase<AccountDailyLoginCheckQueryable, AccountDailyLoginCheckEntity>
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

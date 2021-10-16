using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Account;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.Account;

namespace Scruffy.Data.Entity.Repositories.Account
{
    /// <summary>
    /// Repository for accessing <see cref="GuildWarsAccountEntity"/>
    /// </summary>
    public class AccountRepository : RepositoryBase<AccountQueryable, GuildWarsAccountEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public AccountRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}

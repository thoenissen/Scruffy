using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Account;

namespace Scruffy.Data.Entity.Queryable.Account
{
    /// <summary>
    /// Queryable for accessing the <see cref="AccountDailyLoginCheckEntity"/>
    /// </summary>
    public class AccountDailyLoginCheckQueryable : QueryableBase<AccountDailyLoginCheckEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public AccountDailyLoginCheckQueryable(IQueryable<AccountDailyLoginCheckEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}

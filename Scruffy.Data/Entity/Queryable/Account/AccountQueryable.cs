using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Account;

namespace Scruffy.Data.Entity.Queryable.Account
{
    /// <summary>
    /// Queryable for accessing the <see cref="AccountEntity"/>
    /// </summary>
    public class AccountQueryable : QueryableBase<AccountEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public AccountQueryable(IQueryable<AccountEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}

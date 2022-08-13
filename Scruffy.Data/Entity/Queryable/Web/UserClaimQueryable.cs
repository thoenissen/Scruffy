using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Web;

namespace Scruffy.Data.Entity.Queryable.Web
{
    /// <summary>
    /// Queryable for accessing the <see cref="UserClaimEntity"/>
    /// </summary>
    public class UserClaimQueryable : QueryableBase<UserClaimEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public UserClaimQueryable(IQueryable<UserClaimEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}
using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Web;

namespace Scruffy.Data.Entity.Queryable.Web
{
    /// <summary>
    /// Queryable for accessing the <see cref="UserRoleEntity"/>
    /// </summary>
    public class UserRoleQueryable : QueryableBase<UserRoleEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public UserRoleQueryable(IQueryable<UserRoleEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}
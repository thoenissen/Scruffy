using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Web;

namespace Scruffy.Data.Entity.Queryable.Web
{
    /// <summary>
    /// Queryable for accessing the <see cref="RoleEntity"/>
    /// </summary>
    public class RoleQueryable : QueryableBase<RoleEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public RoleQueryable(IQueryable<RoleEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}
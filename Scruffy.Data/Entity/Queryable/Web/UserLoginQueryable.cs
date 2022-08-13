using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Web;

namespace Scruffy.Data.Entity.Queryable.Web
{
    /// <summary>
    /// Queryable for accessing the <see cref="UserLoginEntity"/>
    /// </summary>
    public class UserLoginQueryable : QueryableBase<UserLoginEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public UserLoginQueryable(IQueryable<UserLoginEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}
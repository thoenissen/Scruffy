using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Web;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Web;

namespace Scruffy.Data.Entity.Repositories.Web
{
    /// <summary>
    /// Repository for accessing <see cref="UserRoleEntity"/>
    /// </summary>
    public class UserRoleRepository : RepositoryBase<UserRoleQueryable, UserRoleEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public UserRoleRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}
using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Queryable.Raid
{
    /// <summary>
    /// Queryable for accessing the <see cref="RaidRequiredRoleEntity"/>
    /// </summary>
    public class RaidRequiredRoleQueryable : QueryableBase<RaidRequiredRoleEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public RaidRequiredRoleQueryable(IQueryable<RaidRequiredRoleEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}

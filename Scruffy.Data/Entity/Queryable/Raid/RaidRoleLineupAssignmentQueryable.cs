using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Queryable.Raid
{
    /// <summary>
    /// Queryable for accessing the <see cref="RaidRoleLineupAssignmentEntity"/>
    /// </summary>
    public class RaidRoleLineupAssignmentQueryable : QueryableBase<RaidRoleLineupAssignmentEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public RaidRoleLineupAssignmentQueryable(IQueryable<RaidRoleLineupAssignmentEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}

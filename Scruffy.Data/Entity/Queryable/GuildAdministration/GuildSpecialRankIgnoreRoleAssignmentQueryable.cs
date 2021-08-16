using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildAdministration;

namespace Scruffy.Data.Entity.Queryable.GuildAdministration
{
    /// <summary>
    /// Queryable for accessing the <see cref="GuildSpecialRankIgnoreRoleAssignmentEntity"/>
    /// </summary>
    public class GuildSpecialRankIgnoreRoleAssignmentQueryable : QueryableBase<GuildSpecialRankIgnoreRoleAssignmentEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public GuildSpecialRankIgnoreRoleAssignmentQueryable(IQueryable<GuildSpecialRankIgnoreRoleAssignmentEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}

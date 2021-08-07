using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Queryable.Raid
{
    /// <summary>
    /// Queryable for accessing the <see cref="RaidRoleLineupHeaderEntity"/>
    /// </summary>
    public class RaidRoleLineupHeaderQueryable : QueryableBase<RaidRoleLineupHeaderEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public RaidRoleLineupHeaderQueryable(IQueryable<RaidRoleLineupHeaderEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}

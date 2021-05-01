using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Queryable.Raid
{
    /// <summary>
    /// Queryable for accessing the <see cref="RaidExperienceLevelEntity"/>
    /// </summary>
    public class RaidExperienceLevelQueryable : QueryableBase<RaidExperienceLevelEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public RaidExperienceLevelQueryable(IQueryable<RaidExperienceLevelEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}

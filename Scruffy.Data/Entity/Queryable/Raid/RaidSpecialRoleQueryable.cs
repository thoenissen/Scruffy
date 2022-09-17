using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Queryable.Raid
{
    /// <summary>
    /// Queryable for accessing the <see cref="RaidSpecialRoleEntity"/>
    /// </summary>
    public class RaidSpecialRoleQueryable : QueryableBase<RaidSpecialRoleEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public RaidSpecialRoleQueryable(IQueryable<RaidSpecialRoleEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}
using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Guild;

namespace Scruffy.Data.Entity.Queryable.Guild
{
    /// <summary>
    /// Queryable for accessing the <see cref="GuildRankAssignmentEntity"/>
    /// </summary>
    public class GuildRankAssignmentQueryable : QueryableBase<GuildRankAssignmentEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public GuildRankAssignmentQueryable(IQueryable<GuildRankAssignmentEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}

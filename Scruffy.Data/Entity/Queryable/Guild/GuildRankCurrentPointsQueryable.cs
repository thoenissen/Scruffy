using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Guild;

namespace Scruffy.Data.Entity.Queryable.Guild
{
    /// <summary>
    /// Queryable for accessing the <see cref="GuildRankCurrentPointsEntity"/>
    /// </summary>
    public class GuildRankCurrentPointsQueryable : QueryableBase<GuildRankCurrentPointsEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public GuildRankCurrentPointsQueryable(IQueryable<GuildRankCurrentPointsEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}

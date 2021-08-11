using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildAdministration;

namespace Scruffy.Data.Entity.Queryable.GuildAdministration
{
    /// <summary>
    /// Queryable for accessing the <see cref="GuildSpecialRankPointsEntity"/>
    /// </summary>
    public class GuildSpecialRankPointsQueryable : QueryableBase<GuildSpecialRankPointsEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public GuildSpecialRankPointsQueryable(IQueryable<GuildSpecialRankPointsEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}

using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.GameData;

namespace Scruffy.Data.Entity.Queryable.GuildWars2.GameData
{
    /// <summary>
    /// Queryable for accessing the <see cref="GuildWarsAchievementRewardEntity"/>
    /// </summary>
    public class GuildWarsAchievementRewardQueryable : QueryableBase<GuildWarsAchievementRewardEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public GuildWarsAchievementRewardQueryable(IQueryable<GuildWarsAchievementRewardEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.Account;

namespace Scruffy.Data.Entity.Queryable.GuildWars2.Account
{
    /// <summary>
    /// Queryable for accessing the <see cref="GuildWarsAccountAchievementBitEntity"/>
    /// </summary>
    public class GuildWarsAccountAchievementBitQueryable : QueryableBase<GuildWarsAccountAchievementBitEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public GuildWarsAccountAchievementBitQueryable(IQueryable<GuildWarsAccountAchievementBitEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}

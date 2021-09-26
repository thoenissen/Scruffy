using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildWars2;

namespace Scruffy.Data.Entity.Queryable.GuildWars2
{
    /// <summary>
    /// Queryable for accessing the <see cref="GuildWarsItemGuildUpgradeConversionEntity"/>
    /// </summary>
    public class GuildWarsItemGuildUpgradeConversionQueryable : QueryableBase<GuildWarsItemGuildUpgradeConversionEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public GuildWarsItemGuildUpgradeConversionQueryable(IQueryable<GuildWarsItemGuildUpgradeConversionEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}

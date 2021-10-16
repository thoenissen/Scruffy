using Microsoft.EntityFrameworkCore;
using Scruffy.Data.Entity.Queryable.GuildWars2.GameData;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.GameData;

namespace Scruffy.Data.Entity.Repositories.GuildWars2.GameData
{
    /// <summary>
    /// Repository for accessing <see cref="GuildWarsItemGuildUpgradeConversionEntity"/>
    /// </summary>
    public class GuildWarsItemGuildUpgradeConversionRepository : RepositoryBase<GuildWarsItemGuildUpgradeConversionQueryable, GuildWarsItemGuildUpgradeConversionEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public GuildWarsItemGuildUpgradeConversionRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}

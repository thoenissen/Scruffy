using Microsoft.EntityFrameworkCore;
using Scruffy.Data.Entity.Queryable.Guild;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Guild;

namespace Scruffy.Data.Entity.Repositories.Guild
{
    /// <summary>
    /// Repository for accessing <see cref="GuildSpecialRankConfigurationEntity"/>
    /// </summary>
    public class GuildSpecialRankConfigurationRepository : RepositoryBase<GuildSpecialRankConfigurationQueryable, GuildSpecialRankConfigurationEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public GuildSpecialRankConfigurationRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}

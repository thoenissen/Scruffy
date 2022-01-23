using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.CoreData;
using Scruffy.Data.Entity.Queryable.Discord;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Discord;

namespace Scruffy.Data.Entity.Repositories.Discord
{
    /// <summary>
    /// Repository for accessing <see cref="BlockedDiscordChannelEntity"/>
    /// </summary>
    public class BlockedDiscordChannelRepository : RepositoryBase<BlockedDiscordChannelQueryable, BlockedDiscordChannelEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public BlockedDiscordChannelRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}

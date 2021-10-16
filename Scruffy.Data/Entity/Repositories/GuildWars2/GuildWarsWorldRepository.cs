using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.GuildWars2;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildWars2;
using Scruffy.Data.Entity.Tables.GuildWars2.GameData;

namespace Scruffy.Data.Entity.Repositories.GuildWars2
{
    /// <summary>
    /// Repository for accessing <see cref="GuildWarsWorldEntity"/>
    /// </summary>
    public class GuildWarsWorldRepository : RepositoryBase<GuildWarsWorldQueryable, GuildWarsWorldEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public GuildWarsWorldRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}

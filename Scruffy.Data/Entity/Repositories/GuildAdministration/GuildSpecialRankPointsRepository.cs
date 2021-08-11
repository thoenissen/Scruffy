using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.GuildAdministration;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildAdministration;

namespace Scruffy.Data.Entity.Repositories.GuildAdministration
{
    /// <summary>
    /// Repository for accessing <see cref="GuildSpecialRankPointsEntity"/>
    /// </summary>
    public class GuildSpecialRankPointsRepository : RepositoryBase<GuildSpecialRankPointsQueryable, GuildSpecialRankPointsEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public GuildSpecialRankPointsRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}

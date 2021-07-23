using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.GuildAdministration;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildAdministration;

namespace Scruffy.Data.Entity.Repositories.GuildAdministration
{
    /// <summary>
    /// Repository for accessing <see cref="GuildLogEntryEntity"/>
    /// </summary>
    public class GuildLogEntryRepository : RepositoryBase<GuildLogEntryQueryable, GuildLogEntryEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public GuildLogEntryRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}

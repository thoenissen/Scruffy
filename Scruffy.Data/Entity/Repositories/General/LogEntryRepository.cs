using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.General;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.General;

namespace Scruffy.Data.Entity.Repositories.General
{
    /// <summary>
    /// Repository for accessing <see cref="LogEntryEntity"/>
    /// </summary>
    public class LogEntryRepository : RepositoryBase<LogEntryQueryable, LogEntryEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public LogEntryRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}

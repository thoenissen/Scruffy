using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Reminder;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Reminder;

namespace Scruffy.Data.Entity.Repositories.Reminder
{
    /// <summary>
    /// Repository for accessing <see cref="OneTimeReminderEntity"/>
    /// </summary>
    public class OneTimeReminderRepository : RepositoryBase<OneTimeReminderQueryable, OneTimeReminderEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public OneTimeReminderRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}

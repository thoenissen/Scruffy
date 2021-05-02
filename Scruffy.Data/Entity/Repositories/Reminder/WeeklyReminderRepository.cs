using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Reminder;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Reminder;

namespace Scruffy.Data.Entity.Repositories.Reminder
{
    /// <summary>
    /// Repository for accessing <see cref="WeeklyReminderEntity"/>
    /// </summary>
    public class WeeklyReminderRepository : RepositoryBase<WeeklyReminderQueryable, WeeklyReminderEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public WeeklyReminderRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}

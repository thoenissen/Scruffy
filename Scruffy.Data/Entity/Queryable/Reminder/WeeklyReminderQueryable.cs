using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Reminder;

namespace Scruffy.Data.Entity.Queryable.Reminder
{
    /// <summary>
    /// Queryable for accessing the <see cref="WeeklyReminderEntity"/>
    /// </summary>
    public class WeeklyReminderQueryable : QueryableBase<WeeklyReminderEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public WeeklyReminderQueryable(IQueryable<WeeklyReminderEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}

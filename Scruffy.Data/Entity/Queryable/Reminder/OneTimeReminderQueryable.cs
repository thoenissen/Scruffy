using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Reminder;

namespace Scruffy.Data.Entity.Queryable.Reminder
{
    /// <summary>
    /// Queryable for accessing the <see cref="OneTimeReminderEntity"/>
    /// </summary>
    public class OneTimeReminderQueryable : QueryableBase<OneTimeReminderEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public OneTimeReminderQueryable(IQueryable<OneTimeReminderEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}

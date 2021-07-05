using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Calendar;

namespace Scruffy.Data.Entity.Queryable.Calendar
{
    /// <summary>
    /// Queryable for accessing the <see cref="CalendarAppointmentScheduleEntity"/>
    /// </summary>
    public class CalendarAppointmentScheduleQueryable : QueryableBase<CalendarAppointmentScheduleEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public CalendarAppointmentScheduleQueryable(IQueryable<CalendarAppointmentScheduleEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}

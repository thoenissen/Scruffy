using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Calendar;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Calendar;

namespace Scruffy.Data.Entity.Repositories.Calendar
{
    /// <summary>
    /// Repository for accessing <see cref="CalendarAppointmentEntity"/>
    /// </summary>
    public class CalendarAppointmentRepository : RepositoryBase<CalendarAppointmentQueryable, CalendarAppointmentEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public CalendarAppointmentRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Calendar;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Calendar;

namespace Scruffy.Data.Entity.Repositories.Calendar
{
    /// <summary>
    /// Repository for accessing <see cref="CalendarAppointmentParticipantEntity"/>
    /// </summary>
    public class CalendarAppointmentParticipantRepository : RepositoryBase<CalendarAppointmentParticipantQueryable, CalendarAppointmentParticipantEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public CalendarAppointmentParticipantRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}

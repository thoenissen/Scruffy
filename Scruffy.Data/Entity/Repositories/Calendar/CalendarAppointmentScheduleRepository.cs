using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Calendar;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Calendar;

namespace Scruffy.Data.Entity.Repositories.Calendar;

/// <summary>
/// Repository for accessing <see cref="CalendarAppointmentScheduleEntity"/>
/// </summary>
public class CalendarAppointmentScheduleRepository : RepositoryBase<CalendarAppointmentScheduleQueryable, CalendarAppointmentScheduleEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public CalendarAppointmentScheduleRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor
}
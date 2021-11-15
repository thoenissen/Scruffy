using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Calendar;

namespace Scruffy.Data.Entity.Queryable.Calendar;

/// <summary>
/// Queryable for accessing the <see cref="CalendarAppointmentParticipantEntity"/>
/// </summary>
public class CalendarAppointmentParticipantQueryable : QueryableBase<CalendarAppointmentParticipantEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public CalendarAppointmentParticipantQueryable(IQueryable<CalendarAppointmentParticipantEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}
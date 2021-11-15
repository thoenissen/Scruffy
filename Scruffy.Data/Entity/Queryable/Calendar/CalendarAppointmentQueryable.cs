using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Calendar;

namespace Scruffy.Data.Entity.Queryable.Calendar;

/// <summary>
/// Queryable for accessing the <see cref="CalendarAppointmentEntity"/>
/// </summary>
public class CalendarAppointmentQueryable : QueryableBase<CalendarAppointmentEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public CalendarAppointmentQueryable(IQueryable<CalendarAppointmentEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}
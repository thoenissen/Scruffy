using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Calendar;

namespace Scruffy.Data.Entity.Queryable.Calendar;

/// <summary>
/// Queryable for accessing the <see cref="CalendarAppointmentTemplateEntity"/>
/// </summary>
public class CalendarAppointmentTemplateQueryable : QueryableBase<CalendarAppointmentTemplateEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public CalendarAppointmentTemplateQueryable(IQueryable<CalendarAppointmentTemplateEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}
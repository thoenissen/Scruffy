using Scruffy.Data.Enumerations.Calendar;

namespace Scruffy.Data.Services.Calendar;

/// <summary>
/// Schedule data
/// </summary>
public class CalenderScheduleData
{
    #region Properties

    /// <summary>
    /// Type
    /// </summary>
    public CalendarAppointmentScheduleType Type { get; set; }

    /// <summary>
    /// Additional data
    /// </summary>
    public string AdditionalData { get; set; }

    #endregion // Properties
}
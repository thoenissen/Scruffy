namespace Scruffy.Data.Enumerations.Calendar;

/// <summary>
/// Special options of <see cref="CalendarAppointmentScheduleType.WeekDayOfMonth"/>
/// </summary>
public enum  WeekDayOfMonthSpecialOptions
{
    /// <summary>
    /// None
    /// </summary>
    None,

    /// <summary>
    /// Only even month
    /// </summary>
    EvenMonth,

    /// <summary>
    /// Only uneven month
    /// </summary>
    UnevenMonth,

    /// <summary>
    /// Selection of months
    /// </summary>
    MonthSelection,
}
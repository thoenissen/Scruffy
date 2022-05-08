using Scruffy.Data.Enumerations.Calendar;

namespace Scruffy.Data.Services.Calendar;

/// <summary>
/// Additional data of the type <see cref="CalendarAppointmentScheduleType.WeekDayOfMonth"/>
/// </summary>
public class WeekDayOfMonthData
{
    #region Propertes

    /// <summary>
    /// Occurence count of the week day
    /// </summary>
    public int OccurenceCount { get; set; }

    /// <summary>
    /// Day of week
    /// </summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>
    /// Options
    /// </summary>
    public WeekDayOfMonthSpecialOptions Options { get; set; }

    /// <summary>
    /// Additional data for the selected option
    /// </summary>
    public string OptionsData { get; set; }

    #endregion // Properties
}
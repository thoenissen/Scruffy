using System;

using Scruffy.Data.Enumerations;

namespace Scruffy.Data.Services.Calendar
{
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

        #endregion // Properties
    }
}

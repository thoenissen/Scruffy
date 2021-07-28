using System;

using Scruffy.Data.Enumerations.Calendar;
using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Services.Calendar.DialogElements.Forms
{
    /// <summary>
    /// Creating a of the type <see cref="CalendarAppointmentScheduleType.WeekDayOfMonth"/>
    /// </summary>
    public class CreateWeekDayOfMonthForm
    {
        #region Propertes

        /// <summary>
        /// Day of week
        /// </summary>
        [DialogElementAssignment(typeof(CalendarScheduleDayOfWeekDialogElement))]
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>
        /// Occurence count of the week day
        /// </summary>
        [DialogElementAssignment(typeof(CalendarScheduleOccurenceCountDialogElement))]
        public int OccurenceCount { get; set; }

        /// <summary>
        /// Options
        /// </summary>
        [DialogElementAssignment(typeof(CalendarScheduleOptionsDialogElement))]
        public WeekDayOfMonthSpecialOptions Options { get; set; }

        #endregion // Properties
    }
}

using System;
using System.Globalization;

namespace Scruffy.Services.Core.Extensions
{
    /// <summary>
    /// DateTime extensions
    /// </summary>
    public static class DateTimeExtensions
    {
        #region Methods

        /// <summary>
        /// Returning ISO 8601 week of year
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>Week number</returns>
        public static int GetIso8601WeekOfYear(this DateTime date)
        {
            var day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                date = date.AddDays(3);
            }

            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        #endregion // Methods
    }
}

using Scruffy.Data.Services.Calendar;
using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Services.Calendar.DialogElements.Forms
{
    /// <summary>
    /// Creation of a new schedule
    /// </summary>
    public class CreateCalendarScheduleData
    {
        #region Properties

        /// <summary>
        /// Description
        /// </summary>
        [DialogElementAssignment(typeof(CalendarScheduleDescriptionDialogElement))]
        public string Description { get; set; }

        /// <summary>
        /// Id of the template
        /// </summary>
        [DialogElementAssignment(typeof(CalendarTemplateSelectionDialogElement))]
        public long TemplateId { get; set; }

        /// <summary>
        /// Schedule
        /// </summary>
        [DialogElementAssignment(typeof(CalendarScheduleScheduleDialogElement))]
        public CalenderScheduleData Schedule { get; set; }

        #endregion // Properties
    }
}

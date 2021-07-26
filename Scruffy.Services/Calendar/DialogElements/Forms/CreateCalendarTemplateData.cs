using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Services.Calendar.DialogElements.Forms
{
    /// <summary>
    /// Creation data of a calendar template
    /// </summary>
    public class CreateCalendarTemplateData
    {
        /// <summary>
        /// Description
        /// </summary>
        [DialogElementAssignment(typeof(CalendarTemplateDescriptionDialogElement))]
        public string Description { get; set; }

        /// <summary>
        /// Uri
        /// </summary>
        [DialogElementAssignment(typeof(CalendarTemplateUriDialogElement))]
        public string Uri { get; set; }

        /// <summary>
        /// Reminder
        /// </summary>
        [DialogElementAssignment(typeof(CalendarTemplateReminderDialogElement))]
        public CalenderTemplateReminderData Reminder { get; set; }

        /// <summary>
        /// Guild points
        /// </summary>
        [DialogElementAssignment(typeof(CalendarTemplateGuildPointsDialogElement))]
        public CalenderTemplateGuildData GuildPoints { get; set; }
    }
}

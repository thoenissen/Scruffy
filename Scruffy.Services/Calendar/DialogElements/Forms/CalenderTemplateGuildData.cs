using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Services.Calendar.DialogElements.Forms
{
    /// <summary>
    /// Guild points
    /// </summary>
    public class CalenderTemplateGuildData
    {
        #region Properties

        /// <summary>
        /// Guild points
        /// </summary>
        [DialogElementAssignment(typeof(CalendarTemplateGuildPointsPointsDialogElement))]
        public int Points { get; set; }

        /// <summary>
        /// Does this event raise the maximum cap of guild points per week?
        /// </summary>
        [DialogElementAssignment(typeof(CalendarTemplateGuildPointsCapDialogElement))]
        public bool IsRaisingPointCap { get; set; }

        #endregion // Properties
    }
}
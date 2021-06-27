using System;

using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Services.Raid.DialogElements.Forms
{
    /// <summary>
    /// Creation of a raid day
    /// </summary>
    public class CreateRaidDayFormData
    {
        #region Properties

        /// <summary>
        /// Alias name
        /// </summary>
        [DialogElementAssignment(typeof(RaidTemplateAliasNameDialogElement))]
        public string AliasName { get; set; }

        /// <summary>
        /// Day of the week
        /// </summary>
        [DialogElementAssignment(typeof(RaidDayDayOfWeekDialogElement))]
        public DayOfWeek Day { get; set; }

        /// <summary>
        /// Registration deadline
        /// </summary>
        [DialogElementAssignment(typeof(RaidDayRegistrationDeadlineDialogElement))]
        public TimeSpan RegistrationDeadline { get; set; }

        /// <summary>
        /// Start of the raid
        /// </summary>
        [DialogElementAssignment(typeof(RaidDayStartTimeDialogElement))]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Id if the first template
        /// </summary>
        [DialogElementAssignment(typeof(RaidTemplateSelectionDialogElement))]
        public long TemplateId { get; set; }

        #endregion // Properties
    }
}

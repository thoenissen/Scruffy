using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Reminder
{
    /// <summary>
    /// Weekly reminder
    /// </summary>
    [Table("WeeklyReminders")]
    public class WeeklyReminderEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Day of week
        /// </summary>
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>
        /// Post time
        /// </summary>
        public TimeSpan PostTime { get; set; }

        /// <summary>
        /// Deletion time
        /// </summary>
        public TimeSpan DeletionTime { get; set; }

        /// <summary>
        /// Id of the channel
        /// </summary>
        public ulong DiscordChannelId { get; set; }

        /// <summary>
        /// Message to be posted
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Id of the posted message
        /// </summary>
        public ulong? DiscordMessageId { get; set; }

        #endregion // Properties
    }
}

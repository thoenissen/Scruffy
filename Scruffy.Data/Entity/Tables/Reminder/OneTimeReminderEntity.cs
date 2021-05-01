using System;
using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.Reminder
{
    /// <summary>
    /// One time reminder
    /// </summary>
    [Table("OneTimeReminders")]
    public class OneTimeReminderEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set;  }

        /// <summary>
        /// Id of the user
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        /// Id of the channel
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        /// Timestamp of the reminder
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Ist the reminder executed?
        /// </summary>
        public bool IsExecuted { get; set; }

        #region Navigation properties

        /// <summary>
        /// User
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; }

        #endregion  // Navigation properties

        #endregion // Properties
    }
}

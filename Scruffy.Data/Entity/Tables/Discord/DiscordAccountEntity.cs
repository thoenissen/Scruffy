using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Scruffy.Data.Entity.Tables.CoreData;
using Scruffy.Data.Entity.Tables.Reminder;

namespace Scruffy.Data.Entity.Tables.Discord
{
    /// <summary>
    /// Discord user
    /// </summary>
    [Table("DiscordAccounts")]
    public class DiscordAccountEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [Key]
        public ulong Id { get; set; }

        /// <summary>
        /// Id of the user
        /// </summary>
        public long UserId { get; set; }

        #region Navigation properties

        /// <summary>
        /// UserId
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; }

        /// <summary>
        /// One time reminders
        /// </summary>
        public virtual ICollection<OneTimeReminderEntity> OneTimeReminders { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.Account;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Data.Entity.Tables.Reminder;

namespace Scruffy.Data.Entity.Tables.CoreData
{
    /// <summary>
    /// User
    /// </summary>
    [Table("Users")]
    public class UserEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [Key]
        public ulong Id { get; set; }

        /// <summary>
        /// Creation of the user
        /// </summary>
        public DateTime CreationTimeStamp { get; set; }

        /// <summary>
        /// Experience level
        /// </summary>
        public long? RaidExperienceLevelId { get; set; }

        #region Navigation properties

        /// <summary>
        /// Raid experience level
        /// </summary>
        public virtual RaidExperienceLevelEntity RaidExperienceLevel { get; set; }

        /// <summary>
        /// One time reminders
        /// </summary>
        public virtual ICollection<OneTimeReminderEntity> OneTimeReminders { get; set; }

        /// <summary>
        /// Raid registrations
        /// </summary>
        public virtual ICollection<RaidRegistrationEntity> RaidRegistrations { get; set; }

        /// <summary>
        /// Raid roles
        /// </summary>
        public virtual ICollection<RaidUserRoleEntity> RaidUserRoles { get; set; }

        /// <summary>
        /// Accounts
        /// </summary>
        public virtual ICollection<AccountEntity> Accounts { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}

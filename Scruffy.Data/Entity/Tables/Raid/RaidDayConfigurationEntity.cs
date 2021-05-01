using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Raid
{
    /// <summary>
    /// Configuration of one raid day
    /// </summary>
    [Table("RaidDayConfigurations")]
    public class RaidDayConfigurationEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Day of the week
        /// </summary>
        public DayOfWeek Day { get; set; }

        /// <summary>
        /// Registration deadline
        /// </summary>
        public TimeSpan RegistrationDeadline { get; set; }

        /// <summary>
        /// Start of the raid
        /// </summary>
        public  TimeSpan StartTime { get; set; }

        /// <summary>
        /// Reset of the raid entry
        /// </summary>
        public TimeSpan ResetTime { get; set; }

        /// <summary>
        /// Reminder
        /// </summary>
        public TimeSpan? ReminderTime { get; set;  }

        /// <summary>
        /// Discord channel where the reminder should be posted
        /// </summary>
        public ulong? ReminderChannelId { get; set; }

        /// <summary>
        /// Discord channel
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        /// Discord message
        /// </summary>
        public ulong MessageId { get; set; }

        /// <summary>
        /// Id of the discord role who administration the entry
        /// </summary>
        public ulong? AdministrationRoleId { get; set; }

        #region Navigation properties

        /// <summary>
        /// Assigned experience roles
        /// </summary>
        public virtual ICollection<RaidExperienceAssignmentEntity> RaidExperienceAssignments { get; set; }

        /// <summary>
        /// Appointments
        /// </summary>
        public virtual ICollection<RaidAppointmentEntity> RaidAppointments { get; set; }

        /// <summary>
        /// Required roles
        /// </summary>
        public virtual ICollection<RaidRequiredRoleEntity> RaidRequiredRoles { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}
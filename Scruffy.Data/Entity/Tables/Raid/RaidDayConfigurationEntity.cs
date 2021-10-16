using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        /// Alias name
        /// </summary>
        [StringLength(20)]
        public string AliasName { get; set; }

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
        /// Discord channel
        /// </summary>
        public ulong DiscordChannelId { get; set; }

        /// <summary>
        /// Discord message
        /// </summary>
        public ulong DiscordMessageId { get; set; }

        #region Navigation properties

        /// <summary>
        /// Appointments
        /// </summary>
        public virtual ICollection<RaidAppointmentEntity> RaidAppointments { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}
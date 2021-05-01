using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Raid
{
    /// <summary>
    /// Appointment
    /// </summary>
    [Table("RaidAppointments")]
    public class RaidAppointmentEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Id of the configuration
        /// </summary>
        public long ConfigurationId { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime TimeStamp { get; set; }

        #region Navigation properties

        /// <summary>
        /// Configuration
        /// </summary>
        [ForeignKey(nameof(ConfigurationId))]
        public virtual RaidDayConfigurationEntity RaidDayConfiguration { get; set; }

        /// <summary>
        /// Registrations
        /// </summary>
        public virtual ICollection<RaidRegistrationEntity> RaidRegistrations { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}
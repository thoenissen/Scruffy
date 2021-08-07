using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.Raid
{
    /// <summary>
    /// Raid registration
    /// </summary>
    [Table("RaidRegistrations")]
    public class RaidRegistrationEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Id of the appointment
        /// </summary>
        public long AppointmentId { get; set; }

        /// <summary>
        /// Id of the user
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        /// Timestamp of the registration
        /// </summary>
        public DateTime RegistrationTimeStamp { get; set; }

        /// <summary>
        /// Points
        /// </summary>
        public long? Points { get; set; }

        /// <summary>
        /// Id of the experience level
        /// </summary>
        public long? LineupExperienceLevelId { get; set; }

        #region Navigation properties

        /// <summary>
        /// Configuration
        /// </summary>
        [ForeignKey(nameof(AppointmentId))]
        public virtual RaidAppointmentEntity RaidAppointment { get; set; }

        /// <summary>
        /// User
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; }

        /// <summary>
        /// Experience level
        /// </summary>
        [ForeignKey(nameof(LineupExperienceLevelId))]
        public virtual RaidExperienceLevelEntity LineupExperienceLevel { get; set; }

        /// <summary>
        /// Registrations
        /// </summary>
        public virtual ICollection<RaidRegistrationRoleAssignmentEntity> RaidRegistrationRoleAssignments { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}
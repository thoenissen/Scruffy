using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.Calendar
{
    /// <summary>
    /// Calendar appointment participant
    /// </summary>
    [Table("CalendarAppointmentParticipants")]
    public class CalendarAppointmentParticipantEntity
    {
        #region Properties

        /// <summary>
        /// Id of appointment
        /// </summary>
        public long AppointmentId { get; set; }

        /// <summary>
        /// Id of the user
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Event leader
        /// </summary>
        public bool IsLeader { get; set; }

        #region Navigation properties

        /// <summary>
        /// Appointment
        /// </summary>
        [ForeignKey(nameof(AppointmentId))]
        public virtual CalendarAppointmentEntity CalendarAppointment { get; set; }

        /// <summary>
        /// User
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}

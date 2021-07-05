using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Calendar
{
    /// <summary>
    /// Appointment
    /// </summary>
    [Table("CalendarAppointments")]
    public class CalendarAppointmentEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Id of the template
        /// </summary>
        public long CalendarAppointmentTemplateId { get; set; }

        #region Navigation properties

        /// <summary>
        /// Template
        /// </summary>
        [ForeignKey(nameof(CalendarAppointmentTemplateId))]
        public virtual CalendarAppointmentTemplateEntity CalendarAppointmentTemplate { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}
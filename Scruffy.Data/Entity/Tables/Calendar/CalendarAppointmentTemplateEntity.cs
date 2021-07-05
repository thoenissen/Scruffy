using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Calendar
{
    /// <summary>
    /// Appointment templates
    /// </summary>
    [Table("CalendarAppointmentTemplates")]
    public class CalendarAppointmentTemplateEntity
    {
        #region Properties

        /// <summary>
        /// Id of template
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Message
        /// </summary>
        public string ReminderMessage { get; set; }

        #region Navigation properties

        /// <summary>
        /// Appointments
        /// </summary>
        public virtual ICollection<CalendarAppointmentEntity> CalendarAppointments { get; set; }

        /// <summary>
        /// Schedules
        /// </summary>
        public virtual ICollection<CalendarAppointmentScheduleEntity> CalendarAppointmentSchedules { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}

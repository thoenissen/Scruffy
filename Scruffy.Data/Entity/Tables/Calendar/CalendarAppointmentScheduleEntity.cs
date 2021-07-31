using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Enumerations.Calendar;

namespace Scruffy.Data.Entity.Tables.Calendar
{
    /// <summary>
    /// Calendar appointment schedule
    /// </summary>
    [Table("CalendarAppointmentSchedules")]
    public class CalendarAppointmentScheduleEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Id of the Server
        /// </summary>
        public ulong ServerId { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Id of the template
        /// </summary>
        public long CalendarAppointmentTemplateId { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public CalendarAppointmentScheduleType Type { get; set; }

        /// <summary>
        /// Additional data
        /// </summary>
        public string AdditionalData { get; set; }

        /// <summary>
        /// Is the schedule deleted?
        /// </summary>
        public bool IsDeleted { get; set; }

        #region Navigation properties

        /// <summary>
        /// Appointment template
        /// </summary>
        [ForeignKey(nameof(CalendarAppointmentTemplateId))]
        public virtual CalendarAppointmentTemplateEntity CalendarAppointmentTemplate { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}
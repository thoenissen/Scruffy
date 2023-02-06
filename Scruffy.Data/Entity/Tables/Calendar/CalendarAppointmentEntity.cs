using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.Calendar;

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

    /// <summary>
    /// Id of the schedule
    /// </summary>
    public long? CalendarAppointmentScheduleId { get; set; }

    /// <summary>
    /// Id of the channel
    /// </summary>
    public ulong? DiscordChannelId { get; set; }

    /// <summary>
    /// Id of the message
    /// </summary>
    public ulong? DiscordMessageId { get; set; }

    /// <summary>
    /// Id of the event
    /// </summary>
    public ulong? DiscordEventId { get; set; }

    /// <summary>
    /// Id of the leader
    /// </summary>
    public long? LeaderId { get; set; }

    #region Navigation properties

    /// <summary>
    /// Template
    /// </summary>
    [ForeignKey(nameof(CalendarAppointmentTemplateId))]
    public virtual CalendarAppointmentTemplateEntity CalendarAppointmentTemplate { get; set; }

    /// <summary>
    /// Schedule
    /// </summary>
    [ForeignKey(nameof(CalendarAppointmentScheduleId))]
    public virtual CalendarAppointmentScheduleEntity CalendarAppointmentSchedule { get; set; }

    /// <summary>
    /// Leader
    /// </summary>
    [ForeignKey(nameof(LeaderId))]
    public virtual UserEntity Leader { get; set; }

    /// <summary>
    /// Participants
    /// </summary>
    public virtual ICollection<CalendarAppointmentParticipantEntity> CalendarAppointmentParticipants { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}
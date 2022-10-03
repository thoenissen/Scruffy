﻿using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.Calendar;

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
    /// Id of the Server
    /// </summary>
    public ulong DiscordServerId { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Reminder time
    /// </summary>
    public TimeSpan AppointmentTime { get; set; }

    /// <summary>
    /// Uri
    /// </summary>
    public string Uri { get; set; }

    /// <summary>
    /// Reminder message
    /// </summary>
    public string ReminderMessage { get; set; }

    /// <summary>
    /// Reminder time
    /// </summary>
    public TimeSpan? ReminderTime { get; set; }

    /// <summary>
    /// Explanation
    /// </summary>
    public string DiscordEventDescription { get; set; }

    /// <summary>
    /// Voice channel
    /// </summary>
    public ulong? DiscordVoiceChannel { get; set; }

    /// <summary>
    /// Guild points
    /// </summary>
    public double? GuildPoints { get; set;  }

    /// <summary>
    /// Does this event raise the maximum cap of guild points per week?
    /// </summary>
    public bool? IsRaisingGuildPointCap { get; set; }

    /// <summary>
    /// Is the template deleted?
    /// </summary>
    public bool IsDeleted { get; set; }

    #region Navigation properties

    /// <summary>
    /// Server
    /// </summary>
    [ForeignKey(nameof(DiscordServerId))]
    public virtual ServerConfigurationEntity ServerConfiguration { get; set; }

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
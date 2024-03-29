﻿using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Services.Calendar.DialogElements.Forms;

/// <summary>
/// Creation data of a calendar template
/// </summary>
public class CreateCalendarTemplateData
{
    #region Properties

    /// <summary>
    /// Description
    /// </summary>
    [DialogElementAssignment(typeof(CalendarTemplateDescriptionDialogElement))]
    public string Description { get; set; }

    /// <summary>
    /// Uri
    /// </summary>
    [DialogElementAssignment(typeof(CalendarTemplateUriDialogElement))]
    public string Uri { get; set; }

    /// <summary>
    /// Appointment time
    /// </summary>
    [DialogElementAssignment(typeof(CalendarTemplateAppointmentTimeDialogElement))]
    public TimeSpan AppointmentTime { get; set; }

    /// <summary>
    /// Reminder
    /// </summary>
    [DialogElementAssignment(typeof(CalendarTemplateReminderDialogElement))]
    public CalenderTemplateReminderData Reminder { get; set; }

    /// <summary>
    /// Guild points
    /// </summary>
    [DialogElementAssignment(typeof(CalendarTemplateGuildPointsDialogElement))]
    public CalenderTemplateGuildData GuildPoints { get; set; }

    #endregion // Properties
}
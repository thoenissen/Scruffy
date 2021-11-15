using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Services.Calendar.DialogElements.Forms;

/// <summary>
/// Reminder
/// </summary>
public class CalenderTemplateReminderData
{
    #region Properties

    /// <summary>
    /// Message
    /// </summary>
    [DialogElementAssignment(typeof(CalendarTemplateReminderMessageDialogElement))]
    public string Message { get; set; }

    /// <summary>
    /// Time
    /// </summary>
    [DialogElementAssignment(typeof(CalendarTemplateReminderTimeDialogElement))]
    public TimeSpan? Time { get; set; }

    #endregion // Properties
}
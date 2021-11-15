using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Services.Calendar.DialogElements.Forms;

/// <summary>
/// Creating a one time event
/// </summary>
public class CreateOneTimeEventFormData
{
    /// <summary>
    /// Day
    /// </summary>
    [DialogElementAssignment(typeof(CalendarOneTimeDayDialogElement))]
    public DateTime Day { get; set; }

    /// <summary>
    /// Id of the template
    /// </summary>
    [DialogElementAssignment(typeof(CalendarTemplateSelectionDialogElement))]
    public long TemplateId { get; set; }
}
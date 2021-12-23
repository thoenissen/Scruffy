using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Services.Guild.DialogElements.Forms;

/// <summary>
/// Setting up the calendar
/// </summary>
public class SetGuildCalendarFormData
{
    #region Properties

    /// <summary>
    /// Title
    /// </summary>
    [DialogElementAssignment(typeof(GuildCalendarTitleDialogElement))]
    public string Title { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    [DialogElementAssignment(typeof(GuildCalendarDescriptionDialogElement))]
    public string Description { get; set; }

    #endregion // Properties
}
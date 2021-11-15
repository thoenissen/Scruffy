using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Services.GuildAdministration.DialogElements.Forms;

/// <summary>
/// Setting up the calendar
/// </summary>
public class SetGuildCalendarFormData
{
    #region Properties

    /// <summary>
    /// Title
    /// </summary>
    [DialogElementAssignment(typeof(GuildAdministrationCalendarTitleDialogElement))]
    public string Title { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    [DialogElementAssignment(typeof(GuildAdministrationCalendarDescriptionDialogElement))]
    public string Description { get; set; }

    #endregion // Properties
}
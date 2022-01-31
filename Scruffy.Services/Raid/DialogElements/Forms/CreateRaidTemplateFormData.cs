using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Services.Raid.DialogElements.Forms;

/// <summary>
/// Data to create a new raid template
/// </summary>
public class CreateRaidTemplateFormData
{
    #region Properties

    /// <summary>
    /// Alias name
    /// </summary>
    [DialogElementAssignment(typeof(RaidTemplateAliasNameDialogElement))]
    public string AliasName { get; set; }

    /// <summary>
    /// Title
    /// </summary>
    [DialogElementAssignment(typeof(RaidTemplateTitleDialogElement))]
    public string Title { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    [DialogElementAssignment(typeof(RaidTemplateDescriptionDialogElement))]
    public string Description { get; set; }

    /// <summary>
    /// Thumbnail
    /// </summary>
    [DialogElementAssignment(typeof(RaidTemplateThumbnailDialogElement))]
    public string Thumbnail { get; set; }

    #endregion // Properties
}
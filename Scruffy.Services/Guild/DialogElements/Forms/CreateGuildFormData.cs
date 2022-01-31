using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Services.Guild.DialogElements.Forms;

/// <summary>
/// Create the guild administration
/// </summary>
public class CreateGuildFormData
{
    #region Properties

    /// <summary>
    /// Api-Key
    /// </summary>
    [DialogElementAssignment(typeof(GuildApiKeyDialogElement))]
    public string ApiKey { get; set; }

    /// <summary>
    /// Id of the Guild
    /// </summary>
    [DialogElementAssignment(typeof(GuildGuildDialogElement))]
    public string GuildId { get; set; }

    #endregion // Properties
}
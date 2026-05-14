using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Services.Guild.DialogElements.Forms;

/// <summary>
/// Edit role form
/// </summary>
public class GuildActivityDiscordMessageEditFormData
{
    #region Properties

    /// <summary>
    /// Id of the role
    /// </summary>
    [DialogElementAssignment(typeof(GuildActivityDiscordMessageEditDialogElement))]
    public ulong RoleId { get; set; }

    /// <summary>
    /// Points
    /// </summary>
    [DialogElementAssignment(typeof(GuildActivityDiscordMessagePointsDialogElement))]
    public double Points { get; set; }

    #endregion // Properties
}
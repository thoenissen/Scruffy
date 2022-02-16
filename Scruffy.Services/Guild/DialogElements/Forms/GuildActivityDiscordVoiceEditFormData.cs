using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Services.Guild.DialogElements.Forms;

/// <summary>
/// Edit role form
/// </summary>
public class GuildActivityDiscordVoiceEditFormData
{
    /// <summary>
    /// Id of the role
    /// </summary>
    [DialogElementAssignment(typeof(GuildActivityDiscordVoiceEditDialogElement))]
    public ulong RoleId { get; set; }

    /// <summary>
    /// Points
    /// </summary>
    [DialogElementAssignment(typeof(GuildActivityDiscordVoicePointsDialogElement))]
    public double Points { get; set; }
}
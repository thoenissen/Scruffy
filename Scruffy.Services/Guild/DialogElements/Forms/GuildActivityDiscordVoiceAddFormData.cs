using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Services.Guild.DialogElements.Forms;

/// <summary>
/// Add role form
/// </summary>
public class GuildActivityDiscordVoiceAddFormData
{
    /// <summary>
    /// Id of the role
    /// </summary>
    [DialogElementAssignment(typeof(GuildActivityDiscordVoiceAddDialogElement))]
    public ulong RoleId { get; set; }

    /// <summary>
    /// Points
    /// </summary>
    [DialogElementAssignment(typeof(GuildActivityDiscordVoicePointsDialogElement))]
    public double Points { get; set; }
}
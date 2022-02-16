using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Services.Guild.DialogElements.Forms;

/// <summary>
/// Add role form
/// </summary>
public class GuildActivityDiscordMessageAddFormData
{
    /// <summary>
    /// Id of the role
    /// </summary>
    [DialogElementAssignment(typeof(GuildActivityDiscordMessageAddDialogElement))]
    public ulong RoleId { get; set; }

    /// <summary>
    /// Points
    /// </summary>
    [DialogElementAssignment(typeof(GuildActivityDiscordMessagePointsDialogElement))]
    public double Points { get; set; }
}
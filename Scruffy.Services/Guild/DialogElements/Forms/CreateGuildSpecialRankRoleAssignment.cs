using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Services.Guild.DialogElements.Forms;

/// <summary>
/// Creating a role assignment
/// </summary>
public class CreateGuildSpecialRankRoleAssignment
{
    /// <summary>
    /// Discord Role
    /// </summary>
    [DialogElementAssignment(typeof(GuildSpecialRankRoleAssignmentDiscordRoleDialogElement))]
    public ulong DiscordRoleId { get; set; }

    /// <summary>
    /// Points
    /// </summary>
    [DialogElementAssignment(typeof(GuildSpecialRankRoleAssignmentPointsDialogElement))]
    public double Points { get; set; }
}
using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Services.GuildAdministration.DialogElements.Forms;

/// <summary>
/// Creating a role assignment
/// </summary>
public class CreateGuildSpecialRankRoleAssignment
{
    /// <summary>
    /// Discord Role
    /// </summary>
    [DialogElementAssignment(typeof(GuildAdministrationSpecialRankRoleAssignmentDiscordRoleDialogElement))]
    public ulong DiscordRoleId { get; set; }

    /// <summary>
    /// Points
    /// </summary>
    [DialogElementAssignment(typeof(GuildAdministrationSpecialRankRoleAssignmentPointsDialogElement))]
    public double Points { get; set; }
}
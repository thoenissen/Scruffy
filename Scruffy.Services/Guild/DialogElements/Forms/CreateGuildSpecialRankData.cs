using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Services.Guild.DialogElements.Forms;

/// <summary>
/// Creation of a guild special rank
/// </summary>
public class CreateGuildSpecialRankData
{
    /// <summary>
    /// Description
    /// </summary>
    [DialogElementAssignment(typeof(GuildSpecialRankDescriptionDialogElement))]
    public string Description { get; set; }

    /// <summary>
    /// Id of the discord role
    /// </summary>
    [DialogElementAssignment(typeof(GuildSpecialRankDiscordRoleDialogElement))]
    public ulong DiscordRoleId { get; set; }

    /// <summary>
    /// Maximum points
    /// </summary>
    [DialogElementAssignment(typeof(GuildSpecialRankMaximumPointsDialogElement))]
    public double MaximumPoints { get; set; }

    /// <summary>
    /// Grand role threshold
    /// </summary>
    [DialogElementAssignment(typeof(GuildSpecialRankGrantThresholdDialogElement))]
    public double GrantThreshold { get; set; }

    /// <summary>
    /// Remove role threshold
    /// </summary>
    [DialogElementAssignment(typeof(GuildSpecialRankRemoveThresholdDialogElement))]
    public double RemoveThreshold { get; set; }
}
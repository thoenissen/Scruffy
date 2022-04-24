using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Services.Guild.DialogElements.Forms;

/// <summary>
/// Item configuration
/// </summary>
public class ItemConfigurationFormData
{
    /// <summary>
    /// Item id
    /// </summary>
    [DialogElementAssignment(typeof(GuildConfigurationItemItemIdDialogElement))]
    public int ItemId { get; set; }

    /// <summary>
    /// Custom value
    /// </summary>
    [DialogElementAssignment(typeof(GuildConfigurationItemCustomValueDialogElement))]
    public long CustomValue { get; set; }

    /// <summary>
    /// Should the value be reduced after n inserts.
    /// </summary>
    [DialogElementAssignment(typeof(GuildConfigurationItemCustomValueThresholdDialogElement))]
    public bool IsThresholdItem { get; set; }
}
using Discord.Interactions;

namespace Scruffy.Data.Enumerations.GuildWars2;

/// <summary>
/// Wizard's Vault display mode
/// </summary>
public enum WizardVaultMode
{
    /// <summary>
    /// Only rewards which can still be purchased
    /// </summary>
    [ChoiceDisplay("Open")]
    Open,

    /// <summary>
    /// Recommended purchases
    /// </summary>
    [ChoiceDisplay("Recommended")]
    Recommended
}
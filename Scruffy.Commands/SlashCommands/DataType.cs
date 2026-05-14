using Discord.Interactions;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Export types
/// </summary>
public enum DataType
{
    /// <summary>
    /// Stash
    /// </summary>
    [ChoiceDisplay("Stash")]
    Stash,

    /// <summary>
    /// Upgrades
    /// </summary>
    [ChoiceDisplay("Upgrades")]
    Upgrades,

    /// <summary>
    /// Login activity
    /// </summary>
    [ChoiceDisplay("Login activity")]
    LoginActivity,

    /// <summary>
    /// Representation
    /// </summary>
    [ChoiceDisplay("Representation")]
    Representation,

    /// <summary>
    /// Representation
    /// </summary>
    [ChoiceDisplay("Members")]
    Members,

    /// <summary>
    /// Roles
    /// </summary>
    [ChoiceDisplay("Roles")]
    Roles,

    /// <summary>
    /// Points
    /// </summary>
    [ChoiceDisplay("Points")]
    Points,

    /// <summary>
    /// Items
    /// </summary>
    [ChoiceDisplay("Items")]
    Items,

    /// <summary>
    /// Assignments
    /// </summary>
    [ChoiceDisplay("Assignments")]
    Assignments
}
using Discord.Interactions;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Check types
/// </summary>
public enum CheckType
{
    /// <summary>
    /// Rank assignments
    /// </summary>
    [ChoiceDisplay("In game rank assignments")]
    RankAssignment,

    /// <summary>
    /// API keys
    /// </summary>
    [ChoiceDisplay("API Keys")]
    ApiKeys,

    /// <summary>
    /// Unknown users
    /// </summary>
    [ChoiceDisplay("Unknown users")]
    UnknownUsers,
}
using Discord.Interactions;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Overview types
/// </summary>
public enum RaidAdminOverviewType
{
    /// <summary>
    /// Participation overview
    /// </summary>
    [ChoiceDisplay("Participation")]
    Participation,

    /// <summary>
    /// Experience levels overview
    /// </summary>
    [ChoiceDisplay("Levels")]
    Levels,

    /// <summary>
    /// Logs overview
    /// </summary>
    [ChoiceDisplay("Logs")]
    Logs
}
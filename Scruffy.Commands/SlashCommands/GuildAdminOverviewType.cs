using Discord.Interactions;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Overview types
/// </summary>
public enum GuildAdminOverviewType
{
    /// <summary>
    /// Ranking overview
    /// </summary>
    [ChoiceDisplay("Ranking")]
    Ranking,

    /// <summary>
    /// Special ranks overview
    /// </summary>
    [ChoiceDisplay("Special ranks")]
    SpecialRanks,

    /// <summary>
    /// Worlds overview
    /// </summary>
    [ChoiceDisplay("Worlds")]
    Worlds
}
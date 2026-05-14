using Discord.Interactions;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Guild ranking visualization type
/// </summary>
public enum GuildRankingVisualizationType
{
    /// <summary>
    /// Current
    /// </summary>
    [ChoiceDisplay("Current points")]
    Current,

    /// <summary>
    /// History per type
    /// </summary>
    [ChoiceDisplay("History per type")]
    HistoryTypes
}
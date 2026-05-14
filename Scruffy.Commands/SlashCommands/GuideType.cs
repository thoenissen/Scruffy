using Discord.Interactions;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Guide type
/// </summary>
public enum GuideType
{
    /// <summary>
    /// Raids
    /// </summary>
    [ChoiceDisplay("Raids")]
    Raids,

    /// <summary>
    /// Strike Missions
    /// </summary>
    [ChoiceDisplay("Strike Missions")]
    StrikeMissions,

    /// <summary>
    /// Fractals of the Mists
    /// </summary>
    [ChoiceDisplay("Fractals of the Mists")]
    Fractals
}
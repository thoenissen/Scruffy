using Discord.Interactions;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Configuration types
/// </summary>
public enum CalendarConfigurationType
{
    /// <summary>
    /// Templates
    /// </summary>
    [ChoiceDisplay("Templates")]
    Templates,

    /// <summary>
    /// Schedules
    /// </summary>
    [ChoiceDisplay("Schedules")]
    Schedules,

    /// <summary>
    /// One time event
    /// </summary>
    [ChoiceDisplay("One time event")]
    OneTimeEvent
}
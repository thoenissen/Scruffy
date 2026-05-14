using Discord.Interactions;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Configuration types
/// </summary>
public enum RaidAdminConfigurationType
{
    /// <summary>
    /// Appointments
    /// </summary>
    [ChoiceDisplay("Appointments")]
    Appointments,

    /// <summary>
    /// Experience levels
    /// </summary>
    [ChoiceDisplay("Experience levels")]
    ExperienceLevels,

    /// <summary>
    /// Templates
    /// </summary>
    [ChoiceDisplay("Templates")]
    Templates
}
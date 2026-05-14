using Discord.Interactions;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Configuration types
/// </summary>
public enum GuildAdminConfigurationType
{
    /// <summary>
    /// General configuration
    /// </summary>
    [ChoiceDisplay("General")]
    General,

    /// <summary>
    /// Guild Wars 2 item configuration
    /// </summary>
    [ChoiceDisplay("Guild Wars 2 item")]
    GuildWarsItems,

    /// <summary>
    /// User configuration
    /// </summary>
    [ChoiceDisplay("User")]
    User,

    /// <summary>
    /// Rank configuration
    /// </summary>
    [ChoiceDisplay("Ranks")]
    Ranks,

    /// <summary>
    /// Special rank configuration
    /// </summary>
    [ChoiceDisplay("Special ranks")]
    SpecialRanks,

    /// <summary>
    /// Message activity roles configuration
    /// </summary>
    [ChoiceDisplay("Message activity roles")]
    MessageActivity,

    /// <summary>
    /// Voice activity roles configuration
    /// </summary>
    [ChoiceDisplay("Voice activity roles")]
    VoiceActivity,

    /// <summary>
    /// Notification channels configuration
    /// </summary>
    [ChoiceDisplay("Notification channels")]
    NotificationChannels,

    /// <summary>
    /// Overview messages configuration
    /// </summary>
    [ChoiceDisplay("Overview")]
    Overviews
}
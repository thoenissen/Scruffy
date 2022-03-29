namespace Scruffy.Data.Enumerations.Guild;

/// <summary>
/// Guild channel configuration
/// </summary>
public enum GuildChannelConfigurationType
{
    /// <summary>
    /// Calendar reminder
    /// </summary>
    CalendarReminder = 1_000,

    /// <summary>
    /// Calendar overview
    /// </summary>
    CalendarOverview = 1_001,

    /// <summary>
    /// Calendar 'Message of the Day'
    /// </summary>
    CalendarMessageOfTheDay = 1_002,

    /// <summary>
    /// Special rank changes
    /// </summary>
    SpecialRankRankChange = 2_000,

    /// <summary>
    /// Guild log import notifications
    /// </summary>
    GuildLogNotification = 3000,

    /// <summary>
    /// Guild rank changes
    /// </summary>
    GuildRankChanges = 4000,
}
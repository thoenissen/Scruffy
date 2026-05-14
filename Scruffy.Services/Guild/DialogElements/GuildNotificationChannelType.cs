namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Channel type
/// </summary>
internal enum GuildNotificationChannelType
{
    /// <summary>
    /// Special rank notification
    /// </summary>
    SpecialRankNotification,

    /// <summary>
    /// Calendar reminder notification
    /// </summary>
    CalendarReminderNotification,

    /// <summary>
    /// Guild log notification
    /// </summary>
    GuildLogNotification,

    /// <summary>
    /// Guild rank change notification
    /// </summary>
    GuildRankChangeNotification,

    /// <summary>
    /// Message of the day
    /// </summary>
    MessageOfTheDay,

    /// <summary>
    /// Calendar
    /// </summary>
    Calendar,

    /// <summary>
    /// User notification
    /// </summary>
    UserNotification
}
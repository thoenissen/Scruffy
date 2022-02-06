namespace Scruffy.Data.Enumerations.General;

/// <summary>
/// Type of log entry
/// </summary>
public enum LogEntryType
{
    /// <summary>
    /// Command error
    /// </summary>
    Command = 1000,

    /// <summary>
    /// Returning job
    /// </summary>
    Job = 2000,

    /// <summary>
    /// Service
    /// </summary>
    Service = 3000,

    /// <summary>
    /// Discord client
    /// </summary>
    DiscordClient = 4000,

    /// <summary>
    /// Commands service
    /// </summary>
    CommandsService = 5000,
}
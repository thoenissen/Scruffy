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
    /// Component interaction error
    /// </summary>
    ComponentInteraction = 1100,

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

    /// <summary>
    /// Interaction service
    /// </summary>
    InteractionService = 6000,
}
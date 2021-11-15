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
}
namespace Scruffy.Data.Enumerations.General;

/// <summary>
/// Log entry level
/// </summary>
public enum LogEntryLevel
{
    /// <summary>
    /// Information
    /// </summary>
    Information = 1000,

    /// <summary>
    /// Warning
    /// </summary>
    Warning = 5000,

    /// <summary>
    /// Critical error
    /// </summary>
    Error = 8000,

    /// <summary>
    /// Critical error
    /// </summary>
    CriticalError = 9000,
}
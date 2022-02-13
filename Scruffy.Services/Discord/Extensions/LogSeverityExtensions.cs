using Discord;

using Scruffy.Data.Enumerations.General;

namespace Scruffy.Services.Discord.Extensions;

/// <summary>
/// <see cref="LogSeverity"/> extensions
/// </summary>
internal static class LogSeverityExtensions
{
    /// <summary>
    /// Returns the matching <see cref="LogEntryLevel"/>
    /// </summary>
    /// <param name="severity">Severity</param>
    /// <returns>The matching level</returns>
    public static LogEntryLevel ToLogEntryLevel(this LogSeverity severity)
    {
        return severity switch
        {
            LogSeverity.Critical => LogEntryLevel.CriticalError,
            LogSeverity.Error => LogEntryLevel.Error,
            LogSeverity.Warning => LogEntryLevel.Warning,
            LogSeverity.Info => LogEntryLevel.Information,
            LogSeverity.Verbose => LogEntryLevel.Verbose,
            LogSeverity.Debug => LogEntryLevel.Debug,
            _ => LogEntryLevel.Warning
        };
    }
}
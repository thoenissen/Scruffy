using Scruffy.Data.Enumerations.General;

using Serilog.Events;

namespace Scruffy.Services.Core.Extensions
{
    /// <summary>
    /// <see cref="LogEntryLevel"/>  extension methods
    /// </summary>
    public static class LogEntryLevelExtensions
    {
        /// <summary>
        /// <see cref="LogEntryLevel"/> to <see cref="LogEventLevel"/>
        /// </summary>
        /// <param name="level"><see cref="LogEntryLevel"/> level</param>
        /// <returns><see cref="LogEventLevel"/> level</returns>
        public static LogEventLevel ToLogEventLevel(this LogEntryLevel level)
        {
            return level switch
            {
                LogEntryLevel.Information   => LogEventLevel.Information,
                LogEntryLevel.Warning       => LogEventLevel.Warning,
                LogEntryLevel.Error         => LogEventLevel.Error,
                LogEntryLevel.CriticalError => LogEventLevel.Fatal,
                _                           => LogEventLevel.Verbose
            };
        }
    }
}

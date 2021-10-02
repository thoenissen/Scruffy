using System;
using System.Diagnostics;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.General;
using Scruffy.Data.Entity.Tables.General;
using Scruffy.Data.Enumerations.General;

namespace Scruffy.Services.Core
{
    /// <summary>
    /// Logging service
    /// </summary>
    public class LoggingService
    {
        #region Fields

        /// <summary>
        /// Internal logger
        /// </summary>
        private static readonly LoggingService _logger;

        /// <summary>
        /// Lock
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// Stop watch
        /// </summary>
        private Stopwatch _stopwatch = Stopwatch.StartNew();

        /// <summary>
        /// Limiting counter
        /// </summary>
        private int _limitCounter;

        /// <summary>
        /// Entry counter
        /// </summary>
        private ulong _entryCounter;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        static LoggingService()
        {
            _logger = new LoggingService();
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Adding a log entry
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="qualifiedCommandName">Command name</param>
        /// <param name="lastUserCommand">Last user commands</param>
        /// <param name="message">Message</param>
        /// <param name="additionalInformation">Additional information</param>
        /// <returns>Log entry id</returns>
        public static long? AddCommandLogEntry(LogEntryLevel level, string qualifiedCommandName, string lastUserCommand, string message, string additionalInformation = null)
        {
            return _logger.WriteLine(LogEntryType.CommandError, level, qualifiedCommandName, lastUserCommand, message, additionalInformation);
        }

        /// <summary>
        /// Adding a log entry
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="className">Class name</param>
        /// <param name="message">Message</param>
        /// <param name="additionalInformation">Additional information</param>
        /// <returns>Log entry id</returns>
        public static long? AddJobLogEntry(LogEntryLevel level, string className, string message, string additionalInformation = null)
        {
            return _logger.WriteLine(LogEntryType.Job, level, className, null, message, additionalInformation);
        }

        /// <summary>
        /// Adding a log entry
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="className">Class name</param>
        /// <param name="action">Action</param>
        /// <param name="message">Message</param>
        /// <param name="additionalInformation">Additional information</param>
        /// <returns>Log entry id</returns>
        public static long? AddJobLogEntry(LogEntryLevel level, string className, string action, string message, string additionalInformation = null)
        {
            return _logger.WriteLine(LogEntryType.Job, level, className, action, message, additionalInformation);
        }

        /// <summary>
        /// Adding a log entry
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="className">Class name</param>
        /// <param name="message">Message</param>
        /// <param name="additionalInformation">Additional information</param>
        /// <returns>Log entry id</returns>
        public static long? AddServiceLogEntry(LogEntryLevel level, string className, string message, string additionalInformation = null)
        {
            return _logger.WriteLine(LogEntryType.Service, level, className, null, message, additionalInformation);
        }

        /// <summary>
        /// Adding a log entry
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="className">Class name</param>
        /// <param name="action">Action</param>
        /// <param name="message">Message</param>
        /// <param name="additionalInformation">Additional information</param>
        /// <returns>Log entry id</returns>
        public static long? AddServiceLogEntry(LogEntryLevel level, string className, string action, string message, string additionalInformation = null)
        {
            return _logger.WriteLine(LogEntryType.Service, level, className, action, message, additionalInformation);
        }

        /// <summary>
        /// Write line to log
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="level">Level</param>
        /// <param name="source">Source</param>
        /// <param name="subSource">Sub source</param>
        /// <param name="message">Message</param>
        /// <param name="additionalInformation">Additional Information</param>
        /// <returns>Log entry id</returns>
        private long? WriteLine(LogEntryType type, LogEntryLevel level, string source, string subSource, string message, string additionalInformation)
        {
            long? logEntryId = null;

            lock (_lock)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{++_entryCounter}] [{level}] [{source}{(string.IsNullOrWhiteSpace(subSource) ? string.Empty : "|")}{(string.IsNullOrWhiteSpace(subSource) ? string.Empty : subSource)}]: {message}");
                if (string.IsNullOrWhiteSpace(additionalInformation) == false)
                {
                    Console.WriteLine($"[{_entryCounter}] -->{Environment.NewLine}{additionalInformation}{Environment.NewLine}[{_entryCounter}] <--");
                }

                if (_stopwatch.Elapsed > TimeSpan.FromMinutes(1))
                {
                    _limitCounter = 0;
                }

                if (++_limitCounter < 120
                 && (int)level >= (int)LogEntryLevel.Warning)
                {
                    using (var dbFactory = RepositoryFactory.CreateInstance())
                    {
                        var logEntry = new LogEntryEntity
                                       {
                                           Level = level,
                                           Type = type,
                                           Source = source,
                                           SubSource = subSource,
                                           Message = message,
                                           AdditionalInformation = additionalInformation
                                       };

                        if (dbFactory.GetRepository<LogEntryRepository>()
                                 .Add(logEntry))
                        {
                            logEntryId = logEntry.Id;
                        }
                    }
                }
            }

            return logEntryId;
        }

        #endregion // Methods
    }
}

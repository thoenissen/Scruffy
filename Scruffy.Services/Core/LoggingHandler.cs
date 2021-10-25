using System;
using System.Diagnostics;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.General;
using Scruffy.Data.Entity.Tables.General;
using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core.Extensions;

using Serilog;
using Serilog.Sinks.Elasticsearch;

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
        private readonly object _lock = new ();

        /// <summary>
        /// Stop watch
        /// </summary>
        private Stopwatch _stopwatch = Stopwatch.StartNew();

        /// <summary>
        /// Limiting counter
        /// </summary>
        private int _limitCounter;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        static LoggingService()
        {
            _logger = new LoggingService();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private LoggingService()
        {
            var environment = Environment.GetEnvironmentVariable("SCRUFFY_ENVIRONMENT");
            var elasticSearch = Environment.GetEnvironmentVariable("SCRUFFY_ELASTICSEARCH");

            var configuration =  new LoggerConfiguration()
                                     .Enrich.WithProperty("Environment", environment);

            if (string.IsNullOrWhiteSpace(environment) == false
             || string.IsNullOrWhiteSpace(elasticSearch) == false)
            {
                configuration.WriteTo
                             .Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticSearch))
                                            {
                                                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                                                AutoRegisterTemplate = true,
                                                MinimumLogEventLevel = Serilog.Events.LogEventLevel.Verbose,
                                                IndexFormat = $"scruffy-{DateTime.Now:yyyy-MM}"
                                            });
            }

            configuration.WriteTo
                         .Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

            Log.Logger = configuration.CreateLogger();
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
        /// <param name="ex">Exception</param>
        /// <returns>Log entry id</returns>
        public static long? AddCommandLogEntry(LogEntryLevel level, string qualifiedCommandName, string lastUserCommand, string message, string additionalInformation = null, Exception ex = null)
        {
            return _logger.WriteLine(LogEntryType.CommandError, level, qualifiedCommandName, lastUserCommand, message, additionalInformation, ex);
        }

        /// <summary>
        /// Adding a log entry
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="className">Class name</param>
        /// <param name="message">Message</param>
        /// <param name="additionalInformation">Additional information</param>
        /// <param name="ex">Exception</param>
        /// <returns>Log entry id</returns>
        public static long? AddJobLogEntry(LogEntryLevel level, string className, string message, string additionalInformation = null, Exception ex = null)
        {
            return _logger.WriteLine(LogEntryType.Job, level, className, null, message, additionalInformation, ex);
        }

        /// <summary>
        /// Adding a log entry
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="className">Class name</param>
        /// <param name="action">Action</param>
        /// <param name="message">Message</param>
        /// <param name="additionalInformation">Additional information</param>
        /// <param name="ex">Exception</param>
        /// <returns>Log entry id</returns>
        public static long? AddJobLogEntry(LogEntryLevel level, string className, string action, string message, string additionalInformation = null, Exception ex = null)
        {
            return _logger.WriteLine(LogEntryType.Job, level, className, action, message, additionalInformation, ex);
        }

        /// <summary>
        /// Adding a log entry
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="className">Class name</param>
        /// <param name="message">Message</param>
        /// <param name="additionalInformation">Additional information</param>
        /// <param name="ex">Exception</param>
        /// <returns>Log entry id</returns>
        public static long? AddServiceLogEntry(LogEntryLevel level, string className, string message, string additionalInformation, Exception ex = null)
        {
            return _logger.WriteLine(LogEntryType.Service, level, className, null, message, additionalInformation, ex);
        }

        /// <summary>
        /// Adding a log entry
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="className">Class name</param>
        /// <param name="action">Action</param>
        /// <param name="message">Message</param>
        /// <param name="additionalInformation">Additional information</param>
        /// <param name="ex">Exception</param>
        /// <returns>Log entry id</returns>
        public static long? AddServiceLogEntry(LogEntryLevel level, string className, string action, string message, string additionalInformation, Exception ex = null)
        {
            return _logger.WriteLine(LogEntryType.Service, level, className, action, message, additionalInformation, ex);
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
        /// <param name="ex">Exception</param>
        /// <returns>Log entry id</returns>
        private long? WriteLine(LogEntryType type, LogEntryLevel level, string source, string subSource, string message, string additionalInformation, Exception ex)
        {
            long? logEntryId = null;

            lock (_lock)
            {
                Log.Logger
                   .ForContext("type", type)
                   .ForContext("source", source)
                   .ForContext("subSource", subSource)
                   .ForContext("message", message)
                   .ForContext("additionalInformation", additionalInformation)
                   .Write(level.ToLogEventLevel(), ex, $"[{source}{(string.IsNullOrWhiteSpace(subSource) ? string.Empty : "|")}{(string.IsNullOrWhiteSpace(subSource) ? string.Empty : subSource)}]: {message}{(string.IsNullOrWhiteSpace(additionalInformation) ? null : " - ")}{additionalInformation}");

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
                                           TimeStamp = DateTime.Now,
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

        /// <summary>
        /// Close and flush
        /// </summary>
        public static void CloseAndFlush()
        {
            Log.CloseAndFlush();
        }

        #endregion // Methods
    }
}

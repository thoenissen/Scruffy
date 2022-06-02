using System.Diagnostics;
using System.Reflection;

using Discord;
using Discord.Interactions;

using OpenSearch.Net;

using Osc;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.General;
using Scruffy.Data.Entity.Tables.General;
using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Discord.Extensions;

namespace Scruffy.Services.Core;

/// <summary>
/// Logging service
/// </summary>
public class LoggingService
{
    #region Fields

    /// <summary>
    /// Internal logger
    /// </summary>
    private static LoggingService _logger;

    /// <summary>
    /// Lock
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// Environment
    /// </summary>
    private readonly string _environment;

    /// <summary>
    /// OpenSearch client
    /// </summary>
    private readonly OpenSearchClient _openSearchClient;

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
    private LoggingService()
    {
        _environment = Environment.GetEnvironmentVariable("SCRUFFY_ENVIRONMENT")?.ToLowerInvariant();
        var openSearchUrl = Environment.GetEnvironmentVariable("SCRUFFY_OPENSEARCH");

        if (string.IsNullOrWhiteSpace(_environment) == false
         && string.IsNullOrWhiteSpace(openSearchUrl) == false)
        {
            var node = new Uri(openSearchUrl);
            var settings = new ConnectionSettings(node);

            settings.ServerCertificateValidationCallback(CertificateValidations.AllowAll);

            var user = Environment.GetEnvironmentVariable("SCRUFFY_OPENSEARCH_USER");

            if (string.IsNullOrWhiteSpace(user) == false)
            {
                settings.BasicAuthentication(user, Environment.GetEnvironmentVariable("SCRUFFY_OPENSEARCH_PASSWORD"));
            }

            _openSearchClient = new OpenSearchClient(settings);
        }
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Initialize
    /// </summary>
    public static void Initialize()
    {
        _logger = new LoggingService();
    }

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
    public static long? AddTextCommandLogEntry(LogEntryLevel level, string qualifiedCommandName, string lastUserCommand, string message, string additionalInformation = null, Exception ex = null)
    {
        // TODO No direct logging to this method. Implementation of logging in IContextContainer.
        return _logger.WriteLine<object>(LogEntryType.Command, level, qualifiedCommandName, lastUserCommand, message, additionalInformation, ex, null);
    }

    /// <summary>
    /// Adding a log entry
    /// </summary>
    /// <param name="level">Level</param>
    /// <param name="customId">Custom id</param>
    /// <param name="message">Message</param>
    /// <param name="additionalInformation">Additional information</param>
    /// <param name="ex">Exception</param>
    /// <returns>Log entry id</returns>
    public static long? AddInteractionLogEntry(LogEntryLevel level, string customId, string message, string additionalInformation, Exception ex = null)
    {
        return _logger.WriteLine<object>(LogEntryType.ComponentInteraction, level, customId, null, message, additionalInformation, ex, null);
    }

    /// <summary>
    /// Adding a log entry
    /// </summary>
    /// <param name="level">Level</param>
    /// <param name="group">Command group</param>
    /// <param name="command">Command name</param>
    /// <param name="message">Message</param>
    /// <param name="additionalInformation">Additional information</param>
    /// <param name="ex">Exception</param>
    /// <returns>Log entry id</returns>
    public static long? AddMessageComponentCommandLogEntry(LogEntryLevel level, string group, string command, string message, string additionalInformation = null, Exception ex = null)
    {
        return _logger.WriteLine<object>(LogEntryType.Command, level, group, command, message, additionalInformation, ex, null);
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
        return _logger.WriteLine<object>(LogEntryType.Job, level, className, null, message, additionalInformation, ex, null);
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
        return _logger.WriteLine<object>(LogEntryType.Job, level, className, action, message, additionalInformation, ex, null);
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
        return _logger.WriteLine<object>(LogEntryType.Service, level, className, null, message, additionalInformation, ex, null);
    }

    /// <summary>
    /// Adding a log entry
    /// </summary>
    /// <param name="level">Level</param>
    /// <param name="className">Class name</param>
    /// <param name="message">Message</param>
    /// <param name="additionalInformation">Additional information</param>
    /// <param name="customData">Custom data</param>
    /// <typeparam name="T">Custom data type</typeparam>
    /// <returns>Log entry id</returns>
    public static long? AddServiceLogEntry<T>(LogEntryLevel level, string className, string message, string additionalInformation, T customData)
    {
        return _logger.WriteLine(LogEntryType.Service, level, className, null, message, additionalInformation, null, customData);
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
        return _logger.WriteLine<object>(LogEntryType.Service, level, className, action, message, additionalInformation, ex, null);
    }

    /// <summary>
    /// Writing <see cref="InteractionService"/> log messages
    /// </summary>
    /// <param name="logMessage">Log message</param>
    public static void AddInteractionServiceLog(LogMessage logMessage)
    {
        _logger.WriteLine<object>(LogEntryType.InteractionService, logMessage.Severity.ToLogEntryLevel(), logMessage.Source, null, logMessage.Message, null, logMessage.Exception, null);
    }

    /// <summary>
    /// Writing <see cref="IDiscordClient"/> log messages
    /// </summary>
    /// <param name="logMessage">Log message</param>
    public static void AddDiscordClientLog(LogMessage logMessage)
    {
        _logger.WriteLine<object>(LogEntryType.DiscordClient, logMessage.Severity.ToLogEntryLevel(), logMessage.Source, null, logMessage.Message, null, logMessage.Exception, null);
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
    /// <param name="customData">Custom data</param>
    /// <returns>Log entry id</returns>
    /// <typeparam name="T">Custom data type</typeparam>
    private long? WriteLine<T>(LogEntryType type, LogEntryLevel level, string source, string subSource, string message, string additionalInformation, Exception ex, T customData)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss} {level}] [{source}{(string.IsNullOrWhiteSpace(subSource) ? string.Empty : "|")}{(string.IsNullOrWhiteSpace(subSource) ? string.Empty : subSource)}]: {message}{(string.IsNullOrWhiteSpace(additionalInformation) ? null : " - ")}{additionalInformation}");

        long? logEntryId = null;

        lock (_lock)
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

            if (_stopwatch.Elapsed > TimeSpan.FromMinutes(1))
            {
                _limitCounter = 0;
            }

            if (++_limitCounter < 120
             && (int)level >= (int)LogEntryLevel.Warning)
            {
                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    if (dbFactory.GetRepository<LogEntryRepository>()
                                 .Add(logEntry))
                    {
                        logEntryId = logEntry.Id;
                    }
                }
            }

            if (_openSearchClient != null)
            {
                var openSearchEntry = new
                                      {
                                          logEntry.TimeStamp,
                                          Level = logEntry.Level.ToString(),
                                          Type = logEntry.Type.ToString(),
                                          logEntry.Source,
                                          logEntry.SubSource,
                                          logEntry.Message,
                                          logEntry.AdditionalInformation,
                                          Environment = _environment,
                                          Application = Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown Application",
                                          Custom = customData
                                      };

                _openSearchClient.Index(openSearchEntry, obj => obj.Index($"scruffy-{_environment}-{DateTime.Today:yyyy-MM}"));
            }
        }

        return logEntryId;
    }

    #endregion // Methods
}
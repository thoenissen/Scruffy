﻿using System.Diagnostics;
using System.Reflection;

using Discord;
using Discord.Commands;
using Discord.Interactions;

using Elasticsearch.Net;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.General;
using Scruffy.Data.Entity.Tables.General;
using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core.Extensions;
using Scruffy.Services.Discord.Extensions;

using Serilog;
using Serilog.Sinks.Elasticsearch;

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
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="configurationAction">Additional configurations</param>
    public static void Initialize(Func<LoggerConfiguration, ILogger> configurationAction = null)
    {
        var environment = Environment.GetEnvironmentVariable("SCRUFFY_ENVIRONMENT");
        var openSearch = Environment.GetEnvironmentVariable("SCRUFFY_OPENSEARCH");

        var configuration = new LoggerConfiguration();

        configuration.Enrich.WithProperty("Environment", environment);

        configuration.Enrich.WithProperty("Application",
                                          Assembly.GetEntryAssembly()
                                                  ?.GetName()
                                                  .Name
                                       ?? "Unknown Application");

        if (string.IsNullOrWhiteSpace(environment) == false
         && string.IsNullOrWhiteSpace(openSearch) == false)
        {
            Func<ConnectionConfiguration, ConnectionConfiguration> modifyConnectionSettings = null;

            var user = Environment.GetEnvironmentVariable("SCRUFFY_OPENSEARCH_USER");

            if (string.IsNullOrWhiteSpace(user) == false)
            {
                modifyConnectionSettings = obj =>
                                           {
                                               obj.BasicAuthentication(user, Environment.GetEnvironmentVariable("SCRUFFY_OPENSEARCH_PASSWORD"));

                                               // HACK / TODO - Create real certificate
                                               obj.ServerCertificateValidationCallback(CertificateValidations.AllowAll);

                                               return obj;
                                           };
            }

            configuration.WriteTo
                         .Elasticsearch(new ElasticsearchSinkOptions(new Uri(openSearch))
                                        {
                                            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                                            AutoRegisterTemplate = true,
                                            MinimumLogEventLevel = Serilog.Events.LogEventLevel.Verbose,
                                            IndexFormat = $"scruffy-{environment}-{{0:yyyy-MM}}",
                                            ModifyConnectionSettings = modifyConnectionSettings
                                        });
        }

        configuration.WriteTo
                     .Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

        if (configurationAction != null)
        {
            Log.Logger = configurationAction.Invoke(configuration);
        }
        else
        {
            Log.Logger = configuration.CreateLogger();
        }
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
        return _logger.WriteLine(LogEntryType.Command, level, qualifiedCommandName, lastUserCommand, message, additionalInformation, ex);
    }

    /// <summary>
    /// Adding a log entry
    /// </summary>
    /// <param name="level">Level</param>
    /// <param name="customId">Custom id</param>
    /// <param name="ex">Exception</param>
    /// <returns>Log entry id</returns>
    public static long? AddComponentInteractionLogEntry(LogEntryLevel level, string customId, Exception ex = null)
    {
        return _logger.WriteLine(LogEntryType.ComponentInteraction, level, customId, null, null, null, ex);
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
        return _logger.WriteLine(LogEntryType.Command, level, group, command, message, additionalInformation, ex);
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
    /// Writing <see cref="CommandService"/> log messages
    /// </summary>
    /// <param name="logMessage">Log message</param>
    public static void AddCommandServiceLog(LogMessage logMessage)
    {
        _logger.WriteLine(LogEntryType.CommandsService, logMessage.Severity.ToLogEntryLevel(), logMessage.Source, null, logMessage.Message, null, logMessage.Exception);
    }

    /// <summary>
    /// Writing <see cref="InteractionService"/> log messages
    /// </summary>
    /// <param name="logMessage">Log message</param>
    public static void AddInteractionServiceLog(LogMessage logMessage)
    {
        _logger.WriteLine(LogEntryType.InteractionService, logMessage.Severity.ToLogEntryLevel(), logMessage.Source, null, logMessage.Message, null, logMessage.Exception);
    }

    /// <summary>
    /// Writing <see cref="IDiscordClient"/> log messages
    /// </summary>
    /// <param name="logMessage">Log message</param>
    public static void AddDiscordClientLog(LogMessage logMessage)
    {
        _logger.WriteLine(LogEntryType.DiscordClient, logMessage.Severity.ToLogEntryLevel(), logMessage.Source, null, logMessage.Message, null, logMessage.Exception);
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
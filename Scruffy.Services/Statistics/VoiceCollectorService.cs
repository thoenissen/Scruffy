using System.Collections.Concurrent;
using System.Threading;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Statistics;
using Scruffy.Data.Entity.Tables.Statistics;
using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core;

namespace Scruffy.Services.Statistics;

/// <summary>
/// Voice time collector
/// </summary>
public sealed class VoiceCollectorService : SingletonLocatedServiceBase, IDisposable
{
    #region Fields

    /// <summary>
    /// Lock factory
    /// </summary>
    private readonly LockFactory _lockFactory = new();

    /// <summary>
    /// Active voice sessions (key: ServerId, ChannelId, AccountId)
    /// </summary>
    private readonly ConcurrentDictionary<(ulong ServerId, ulong ChannelId, ulong AccountId), DateTime> _activeSessions = new();

    /// <summary>
    /// Discord client
    /// </summary>
    private DiscordSocketClient _client;

    /// <summary>
    /// Timer for flushing segments every hour
    /// </summary>
    private Timer _flushTimer;

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Start collecting voice sessions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task StartCollecting()
    {
        await using (await _lockFactory.CreateLockAsync().ConfigureAwait(false))
        {
            _client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
            _client.Disconnected += OnDisconnected;
            _client.Connected += OnConnected;

            _flushTimer = new Timer(OnFlushTimer, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));

            if (_client.ConnectionState == ConnectionState.Connected)
            {
                try
                {
                    ScanCurrentVoiceUsers();
                }
                catch (Exception ex)
                {
                    LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(VoiceCollectorService), "StartCollecting", "Error while starting voice collector service", null, ex);
                }
            }
        }
    }

    /// <summary>
    /// Scan all guilds for users currently in voice channels
    /// </summary>
    private void ScanCurrentVoiceUsers()
    {
        var now = DateTime.Now;

        foreach (var guild in _client.Guilds)
        {
            foreach (var voiceChannel in guild.VoiceChannels)
            {
                foreach (var user in voiceChannel.ConnectedUsers)
                {
                    if (user.IsBot)
                    {
                        continue;
                    }

                    _activeSessions.TryAdd((guild.Id, voiceChannel.Id, user.Id), now);
                }
            }
        }
    }

    /// <summary>
    /// Client connected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnConnected()
    {
        try
        {
            await using (await _lockFactory.CreateLockAsync().ConfigureAwait(false))
            {
                ScanCurrentVoiceUsers();
            }
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(VoiceCollectorService), "OnConnected", "Error while scanning voice channels", null, ex);
        }
    }

    /// <summary>
    /// Client disconnected
    /// </summary>
    /// <param name="arg">Arguments</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private Task OnDisconnected(Exception arg)
    {
        FlushAllSessions();

        return Task.CompletedTask;
    }

    /// <summary>
    /// User voice state updated
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="before">State before</param>
    /// <param name="after">State after</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
    {
        if (user.IsBot)
        {
            return Task.CompletedTask;
        }

        try
        {
            // User left a voice channel
            if (before.VoiceChannel != null)
            {
                var key = (before.VoiceChannel.Guild.Id, before.VoiceChannel.Id, user.Id);

                if (_activeSessions.TryRemove(key, out var start))
                {
                    SaveSegment(key.Item1, key.Item2, user.Id, start, DateTime.Now);
                }
            }

            // User joined a voice channel
            if (after.VoiceChannel != null)
            {
                var key = (after.VoiceChannel.Guild.Id, after.VoiceChannel.Id, user.Id);

                _activeSessions.TryAdd(key, DateTime.Now);
            }
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(VoiceCollectorService), "OnUserVoiceStateUpdated", "Error processing voice state update", null, ex);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Periodically flush active sessions into 1-hour segments
    /// </summary>
    /// <param name="state">Timer state</param>
    private void OnFlushTimer(object state)
    {
        try
        {
            var now = DateTime.Now;

            foreach (var (key, start) in _activeSessions)
            {
                if ((now - start).TotalHours >= 1)
                {
                    if (_activeSessions.TryUpdate(key, now, start))
                    {
                        SaveSegments(key.ServerId, key.ChannelId, key.AccountId, start, now);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(VoiceCollectorService), "OnFlushTimer", "Error during periodic flush", null, ex);
        }
    }

    /// <summary>
    /// Flush all active sessions and clear the dictionary
    /// </summary>
    private void FlushAllSessions()
    {
        var now = DateTime.Now;

        foreach (var kvp in _activeSessions)
        {
            if (_activeSessions.TryRemove(kvp.Key, out var start))
            {
                SaveSegments(kvp.Key.ServerId, kvp.Key.ChannelId, kvp.Key.AccountId, start, now);
            }
        }
    }

    /// <summary>
    /// Save a time span, splitting it into segments of at most one hour
    /// </summary>
    /// <param name="serverId">Server ID</param>
    /// <param name="channelId">Channel ID</param>
    /// <param name="accountId">Account ID</param>
    /// <param name="start">Start time</param>
    /// <param name="end">End time</param>
    private void SaveSegments(ulong serverId, ulong channelId, ulong accountId, DateTime start, DateTime end)
    {
        var current = start;

        while (current < end)
        {
            var segmentEnd = current.AddHours(1);

            if (segmentEnd > end)
            {
                segmentEnd = end;
            }

            SaveSegment(serverId, channelId, accountId, current, segmentEnd);

            current = segmentEnd;
        }
    }

    /// <summary>
    /// Save a single voice time span segment to the database
    /// </summary>
    /// <param name="serverId">Server ID</param>
    /// <param name="channelId">Channel ID</param>
    /// <param name="accountId">Account ID</param>
    /// <param name="start">Start time</param>
    /// <param name="end">End time</param>
    private void SaveSegment(ulong serverId, ulong channelId, ulong accountId, DateTime start, DateTime end)
    {
        try
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                if (dbFactory.GetRepository<DiscordVoiceTimeSpanRepository>()
                             .Add(new DiscordVoiceTimeSpanEntity
                                  {
                                      DiscordServerId = serverId,
                                      DiscordChannelId = channelId,
                                      DiscordAccountId = accountId,
                                      StartTimeStamp = start,
                                      EndTimeStamp = end,
                                      IsCompleted = true
                                  })
                 == false)
                {
                    LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(VoiceCollectorService), "SaveSegment", dbFactory.LastError?.Message, dbFactory.LastError?.ToString());
                }
            }
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(VoiceCollectorService), "SaveSegment", "Error saving voice segment", null, ex);
        }
    }

    #endregion // Methods

    #region SingletonLocatedServiceBase

    /// <inheritdoc />
    public override Task Initialize(IServiceProvider serviceProvider)
    {
        _client = serviceProvider.GetService<DiscordSocketClient>();

        Task.Run(StartCollecting);

        return base.Initialize(serviceProvider);
    }

    #endregion // SingletonLocatedServiceBase

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        _flushTimer?.Dispose();

        FlushAllSessions();

        _lockFactory.Dispose();
    }

    #endregion // IDisposable
}
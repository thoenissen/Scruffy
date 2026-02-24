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
    /// Segment duration in minutes (aligned to 00, 10, 20, 30, 40, 50)
    /// </summary>
    private const int SegmentDurationMinutes = 10;

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
    /// Timer for flushing segments periodically
    /// </summary>
    private Timer _flushTimer;

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Calculate the next 10-minute boundary after the given time
    /// </summary>
    /// <param name="time">Reference time</param>
    /// <returns>The next aligned boundary (e.g. :00, :10, :20, :30, :40, :50)</returns>
    private static DateTime GetNextSegmentBoundary(DateTime time)
    {
        var nextBoundaryMinute = ((time.Minute / SegmentDurationMinutes) + 1) * SegmentDurationMinutes;
        var result = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0, time.Kind);

        return result.AddMinutes(nextBoundaryMinute);
    }

    /// <summary>
    /// Start collecting voice sessions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task StartCollecting()
    {
        await using (await _lockFactory.CreateLockAsync().ConfigureAwait(false))
        {
            try
            {
                RecoverIncompleteSessions();
            }
            catch (Exception ex)
            {
                LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(VoiceCollectorService), "StartCollecting", "Error recovering incomplete sessions", null, ex);
            }

            _client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
            _client.Disconnected += OnDisconnected;
            _client.Connected += OnConnected;

            _flushTimer = new Timer(OnFlushTimer, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            if (_client.ConnectionState == ConnectionState.Connected)
            {
                try
                {
                    ScanCurrentVoiceUsers();
                }
                catch (Exception ex)
                {
                    LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(VoiceCollectorService), "StartCollecting", "Error while scanning voice channels", null, ex);
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

                    var key = (guild.Id, voiceChannel.Id, user.Id);

                    if (_activeSessions.TryAdd(key, now))
                    {
                        BeginSegment(guild.Id, voiceChannel.Id, user.Id, now);
                    }
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

                if (_activeSessions.TryRemove(key, out var segmentStart))
                {
                    CompleteSegment(key.Item1, key.Item2, user.Id, segmentStart, DateTime.Now);
                }
            }

            // User joined a voice channel
            if (after.VoiceChannel != null)
            {
                var key = (after.VoiceChannel.Guild.Id, after.VoiceChannel.Id, user.Id);
                var now = DateTime.Now;

                if (_activeSessions.TryAdd(key, now))
                {
                    BeginSegment(key.Item1, key.Item2, user.Id, now);
                }
            }
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(VoiceCollectorService), "OnUserVoiceStateUpdated", "Error processing voice state update", null, ex);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Periodically complete segments that have crossed a 10-minute boundary
    /// </summary>
    /// <param name="state">Timer state</param>
    private void OnFlushTimer(object state)
    {
        try
        {
            var now = DateTime.Now;

            foreach (var (key, segmentStart) in _activeSessions)
            {
                var nextBoundary = GetNextSegmentBoundary(segmentStart);

                if (now >= nextBoundary)
                {
                    if (_activeSessions.TryUpdate(key, nextBoundary, segmentStart))
                    {
                        CompleteSegment(key.ServerId, key.ChannelId, key.AccountId, segmentStart, nextBoundary);

                        BeginSegment(key.ServerId, key.ChannelId, key.AccountId, nextBoundary);
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
            if (_activeSessions.TryRemove(kvp.Key, out var segmentStart))
            {
                CompleteSegment(kvp.Key.ServerId, kvp.Key.ChannelId, kvp.Key.AccountId, segmentStart, now);
            }
        }
    }

    /// <summary>
    /// Recover incomplete sessions left over from a previous unclean shutdown
    /// </summary>
    private void RecoverIncompleteSessions()
    {
        var activeVoiceUsers = new HashSet<(ulong ServerId, ulong ChannelId, ulong AccountId)>();

        if (_client.ConnectionState == ConnectionState.Connected)
        {
            foreach (var guild in _client.Guilds)
            {
                foreach (var voiceChannel in guild.VoiceChannels)
                {
                    foreach (var user in voiceChannel.ConnectedUsers)
                    {
                        if (user.IsBot == false)
                        {
                            activeVoiceUsers.Add((guild.Id, voiceChannel.Id, user.Id));
                        }
                    }
                }
            }
        }

        var now = DateTime.Now;

        List<DiscordVoiceTimeSpanEntity> incompleteRecords;

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            incompleteRecords = dbFactory.GetRepository<DiscordVoiceTimeSpanRepository>()
                                         .GetQuery()
                                         .Where(e => e.IsCompleted == false)
                                         .ToList();
        }

        foreach (var record in incompleteRecords)
        {
            var key = (record.DiscordServerId, record.DiscordChannelId, record.DiscordAccountId);
            var maxEnd = record.StartTimeStamp.AddMinutes(SegmentDurationMinutes);
            var endTime = maxEnd < now ? maxEnd : now;

            CompleteSegment(record.DiscordServerId, record.DiscordChannelId, record.DiscordAccountId, record.StartTimeStamp, endTime);

            if (activeVoiceUsers.Remove(key))
            {
                _activeSessions.TryAdd(key, now);
                BeginSegment(key.DiscordServerId, key.DiscordChannelId, key.DiscordAccountId, now);
            }
        }
    }

    /// <summary>
    /// Begin a new segment by writing an incomplete record to the database
    /// </summary>
    /// <param name="serverId">Server ID</param>
    /// <param name="channelId">Channel ID</param>
    /// <param name="accountId">Account ID</param>
    /// <param name="segmentStart">Start time of the segment</param>
    private void BeginSegment(ulong serverId, ulong channelId, ulong accountId, DateTime segmentStart)
    {
        try
        {
            using var dbFactory = RepositoryFactory.CreateInstance();

            if (dbFactory.GetRepository<DiscordVoiceTimeSpanRepository>()
                         .Add(new DiscordVoiceTimeSpanEntity
                              {
                                  DiscordServerId = serverId,
                                  DiscordChannelId = channelId,
                                  DiscordAccountId = accountId,
                                  StartTimeStamp = segmentStart,
                                  EndTimeStamp = segmentStart,
                                  IsCompleted = false
                              })
             == false)
            {
                LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(VoiceCollectorService), nameof(BeginSegment), dbFactory.LastError?.Message, dbFactory.LastError?.ToString());
            }
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(VoiceCollectorService), nameof(BeginSegment), "Error creating voice segment", null, ex);
        }
    }

    /// <summary>
    /// Complete an existing segment, splitting at 10-minute boundaries if the time span crosses them
    /// </summary>
    /// <param name="serverId">Server ID</param>
    /// <param name="channelId">Channel ID</param>
    /// <param name="accountId">Account ID</param>
    /// <param name="segmentStart">Start time of the segment (part of the primary key)</param>
    /// <param name="segmentEnd">End time of the segment</param>
    private void CompleteSegment(ulong serverId, ulong channelId, ulong accountId, DateTime segmentStart, DateTime segmentEnd)
    {
        var nextBoundary = GetNextSegmentBoundary(segmentStart);

        if (segmentEnd <= nextBoundary)
        {
            FinalizeSegment(serverId, channelId, accountId, segmentStart, segmentEnd);

            return;
        }

        // Complete the first segment at the boundary
        FinalizeSegment(serverId, channelId, accountId, segmentStart, nextBoundary);

        // Create and complete intermediate segments for each crossed boundary
        var current = nextBoundary;

        while (current < segmentEnd)
        {
            var end = GetNextSegmentBoundary(current);

            if (end > segmentEnd)
            {
                end = segmentEnd;
            }

            BeginSegment(serverId, channelId, accountId, current);
            FinalizeSegment(serverId, channelId, accountId, current, end);

            current = end;
        }
    }

    /// <summary>
    /// Write the completion of a single segment to the database
    /// </summary>
    /// <param name="serverId">Server ID</param>
    /// <param name="channelId">Channel ID</param>
    /// <param name="accountId">Account ID</param>
    /// <param name="segmentStart">Start time of the segment (part of the primary key)</param>
    /// <param name="segmentEnd">End time of the segment</param>
    private void FinalizeSegment(ulong serverId, ulong channelId, ulong accountId, DateTime segmentStart, DateTime segmentEnd)
    {
        try
        {
            using var dbFactory = RepositoryFactory.CreateInstance();

            if (dbFactory.GetRepository<DiscordVoiceTimeSpanRepository>()
                         .Refresh(e => e.DiscordServerId == serverId
                                       && e.DiscordChannelId == channelId
                                       && e.DiscordAccountId == accountId
                                       && e.StartTimeStamp == segmentStart,
                                  e =>
                                  {
                                      e.EndTimeStamp = segmentEnd;
                                      e.IsCompleted = true;
                                  }) == false)
            {
                LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(VoiceCollectorService), nameof(FinalizeSegment), dbFactory.LastError?.Message, dbFactory.LastError?.ToString());
            }
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(VoiceCollectorService), nameof(FinalizeSegment), "Error completing voice segment", null, ex);
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
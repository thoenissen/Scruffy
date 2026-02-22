using Discord;
using Discord.WebSocket;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Discord.Jobs;

/// <summary>
/// Importing discord channel names
/// </summary>
public class DiscordChannelImportJob : LocatedAsyncJob
{
    #region Fields

    /// <summary>
    /// Client
    /// </summary>
    private readonly DiscordSocketClient _client;

    /// <summary>
    /// Factory
    /// </summary>
    private readonly RepositoryFactory _dbFactory;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="client">Client</param>
    /// <param name="dbFactory">Factory</param>
    public DiscordChannelImportJob(DiscordSocketClient client, RepositoryFactory dbFactory)
    {
        _client = client;
        _dbFactory = dbFactory;
    }

    #endregion // Constructor

    #region LocatedAsyncJob

    /// <inheritdoc/>
    public override async Task ExecuteOverrideAsync()
    {
        var entries = new List<(ulong ServerId, ulong ChannelId, string Name)>();

        foreach (var guild in _client.Guilds)
        {
            foreach (var channel in guild.Channels)
            {
                if (channel is ICategoryChannel)
                {
                    continue;
                }

                entries.Add((guild.Id, channel.Id, channel.Name));
            }
        }

        if (await _dbFactory.GetRepository<DiscordServerChannelRepository>()
                            .BulkInsert(entries)
                            .ConfigureAwait(false) == false)
        {
            LoggingService.AddJobLogEntry(LogEntryLevel.Error, nameof(DiscordChannelImportJob), "Discord channels import failed", null, _dbFactory.LastError);
        }
    }

    #endregion // LocatedAsyncJob
}
using Discord.WebSocket;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Extensions;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Discord.Jobs;

/// <summary>
/// Importing discord members
/// </summary>
public class DiscordMemberImportJob : LocatedAsyncJob
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
    public DiscordMemberImportJob(DiscordSocketClient client, RepositoryFactory dbFactory)
    {
        _client = client;
        _dbFactory = dbFactory;
    }

    #endregion // Constructor

    #region LocatedAsyncJob

    /// <inheritdoc/>
    public override async Task ExecuteOverrideAsync()
    {
        var entries = new List<(ulong ServerId, ulong AccountId, string Name)>();

        foreach (var guild in _client.Guilds)
        {
            foreach (var user in guild.Users)
            {
                entries.Add((guild.Id, user.Id, user.TryGetDisplayName()));
            }
        }

        if (await _dbFactory.GetRepository<DiscordServerMemberRepository>()
                            .BulkInsert(entries)
                            .ConfigureAwait(false) == false)
        {
            LoggingService.AddJobLogEntry(LogEntryLevel.Error, nameof(DiscordRoleImportJob), "Discord members import failed", null, _dbFactory.LastError);
        }
    }

    #endregion // LocatedAsyncJob
}
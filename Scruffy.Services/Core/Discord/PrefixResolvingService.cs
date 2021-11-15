using System.Collections.Concurrent;

using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;

namespace Scruffy.Services.Core.Discord;

/// <summary>
/// Resolving command prefixes
/// </summary>
public class PrefixResolvingService
{
    #region Fields

    /// <summary>
    /// Prefixes
    /// </summary>
    private ConcurrentDictionary<ulong, string> _prefixes;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public PrefixResolvingService()
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            _prefixes = new ConcurrentDictionary<ulong, string>(dbFactory.GetRepository<ServerConfigurationRepository>()
                                                                         .GetQuery()
                                                                         .ToDictionary(obj => obj.DiscordServerId, obj => obj.Prefix));
        }
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Refresh a server prefix
    /// </summary>
    /// <param name="serverId">Id of the server</param>
    /// <param name="prefix">Prefix</param>
    public void AddOrRefresh(ulong serverId, string prefix)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            if (dbFactory.GetRepository<ServerConfigurationRepository>()
                         .AddOrRefresh(obj => obj.DiscordServerId == serverId,
                                       obj =>
                                       {
                                           obj.DiscordServerId = serverId;
                                           obj.Prefix = prefix;
                                       }))
            {
                _prefixes[serverId] = prefix;
            }
        }
    }

    /// <summary>
    /// Resolving the prefix
    /// </summary>
    /// <param name="msg">Message</param>
    /// <returns>Position</returns>
    public Task<int> OnPrefixResolver(DiscordMessage msg)
    {
        int result;

        if (msg?.Channel?.GuildId != null
         && _prefixes.TryGetValue(msg.Channel.GuildId.Value, out var prefix))
        {
            result = msg.GetStringPrefixLength(prefix, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            result = msg.GetStringPrefixLength(".", StringComparison.OrdinalIgnoreCase);
        }

        return Task.FromResult(result);
    }

    #endregion // Methods
}
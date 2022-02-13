using System.Collections.Concurrent;

using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Services.Core;

namespace Scruffy.Services.Discord;

/// <summary>
/// Resolving command prefixes
/// </summary>
public class PrefixResolvingService : SingletonLocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Prefixes
    /// </summary>
    private ConcurrentDictionary<ulong, string> _prefixes;

    #endregion // Fields

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
    public string GetPrefix(IUserMessage msg)
    {
        if (msg?.Channel is not IGuildChannel channel
         || _prefixes.TryGetValue(channel.Guild.Id, out var prefix) == false)
        {
            prefix = ".";
        }

        return prefix;
    }

    #endregion // Methods

    #region SingletonLocatedServiceBase

    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <remarks>When this method is called all services are registered and can be resolved.  But not all singleton services may be initialized. </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task Initialize(IServiceProvider serviceProvider)
    {
        await base.Initialize(serviceProvider)
                  .ConfigureAwait(false);

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            _prefixes = new ConcurrentDictionary<ulong, string>(dbFactory.GetRepository<ServerConfigurationRepository>()
                                                                         .GetQuery()
                                                                         .ToDictionary(obj => obj.DiscordServerId, obj => obj.Prefix));
        }
    }

    #endregion // SingletonLocatedServiceBase
}
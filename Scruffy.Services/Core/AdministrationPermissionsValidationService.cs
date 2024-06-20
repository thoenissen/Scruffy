using System.Collections.Concurrent;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.Services.Core;

/// <summary>
/// Validation of administration permissions
/// </summary>
public class AdministrationPermissionsValidationService : SingletonLocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Administration roles
    /// </summary>
    private ConcurrentDictionary<ulong, ulong> _roles;

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Refresh a server prefix
    /// </summary>
    /// <param name="serverId">Id of the server</param>
    /// <param name="roleId">Id of role</param>
    public void AddOrRefresh(ulong serverId, ulong roleId)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            if (dbFactory.GetRepository<ServerConfigurationRepository>()
                         .AddOrRefresh(obj => obj.DiscordServerId == serverId,
                                       obj =>
                                       {
                                           obj.DiscordServerId = serverId;
                                           obj.DiscordAdministratorRoleId = roleId;
                                       }))
            {
                _roles[serverId] = roleId;
            }
        }
    }

    /// <summary>
    /// Check administrations permissions
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <returns>Are permissions set?</returns>
    public Task<bool> CheckPermissions(IContextContainer commandContext)
    {
        var result = commandContext.User.Id == commandContext.Guild?.OwnerId
                  || commandContext.Member?.GuildPermissions.Administrator == true
                  || (commandContext.Member != null && commandContext.Guild != null && _roles.TryGetValue(commandContext.Guild.Id, out var roleId) && commandContext.Member.RoleIds.Any(obj => obj == roleId));

        return Task.FromResult(result);
    }

    #endregion // Methods

    #region SingletonLocatedServiceBase

    /// <inheritdoc/>
    public override async Task Initialize(IServiceProvider serviceProvider)
    {
        await base.Initialize(serviceProvider)
                  .ConfigureAwait(false);

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            _roles = new ConcurrentDictionary<ulong, ulong>(dbFactory.GetRepository<ServerConfigurationRepository>()
                                                                     .GetQuery()
                                                                     .Where(obj => obj.DiscordAdministratorRoleId != null)

                                                                     // ReSharper disable once PossibleInvalidOperationException
                                                                     .ToDictionary(obj => obj.DiscordServerId, obj => obj.DiscordAdministratorRoleId.Value));
        }
    }

    #endregion // SingletonLocatedServiceBase
}
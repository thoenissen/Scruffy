using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.Web;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;

namespace Scruffy.Services.Web;

/// <summary>
/// User import service
/// </summary>
public class UsersImportService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _dbFactory;

    /// <summary>
    /// User management service
    /// </summary>
    private readonly UserManagementService _userManagementService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="dbFactory">Repository factory</param>
    /// <param name="userManagementService">User management service</param>
    public UsersImportService(LocalizationService localizationService,
                              RepositoryFactory dbFactory,
                              UserManagementService userManagementService)
        : base(localizationService)
    {
        _dbFactory = dbFactory;
        _userManagementService = userManagementService;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Import discord users
    /// </summary>
    /// <param name="discordServer">Discord server</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ImportDiscordUsers(IGuild discordServer)
    {
        var serverConfiguration = _dbFactory.GetRepository<ServerConfigurationRepository>()
                                            .GetQuery()
                                            .Select(obj => obj);

        var roles = _dbFactory.GetRepository<GuildRepository>()
                              .GetQuery()
                              .Where(obj => obj.DiscordServerId == discordServer.Id)
                              .Select(obj => new
                                             {
                                                 obj.MemberDiscordRoleId,
                                                 AdministratorRoleId = serverConfiguration.Where(obj2 => obj2.DiscordServerId == obj.DiscordServerId)
                                                                                          .Select(obj2 => obj2.DiscordAdministratorRoleId)
                                                                                          .FirstOrDefault()
                                             })
                              .FirstOrDefault();

        if (roles?.MemberDiscordRoleId != null
         && roles.AdministratorRoleId != null)
        {
            var users = new List<(long UserId, ulong DiscordAccountId, string Name)>();
            var administrators = new List<long>();

            foreach (var user in await discordServer.GetUsersAsync()
                                                    .ConfigureAwait(false))
            {
                if (user.RoleIds.Contains(roles.MemberDiscordRoleId.Value))
                {
                    var internalUser = await _userManagementService.GetUserByDiscordAccountId(user)
                                                                   .ConfigureAwait(false);

                    if (internalUser != null)
                    {
                        users.Add((internalUser.Id, user.Id, $"{user.Username}#{user.Discriminator}"));

                        if (user.RoleIds.Contains(roles.AdministratorRoleId.Value))
                        {
                            administrators.Add(internalUser.Id);
                        }
                    }
                }
            }

            await _dbFactory.GetRepository<UserLoginRepository>()
                            .BulkInsert(users)
                            .ConfigureAwait(false);

            await _dbFactory.GetRepository<UserRoleRepository>()
                            .BulkInsertAdministrators(administrators)
                            .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Import discord users
    /// </summary>
    /// <param name="discordServer">Discord server</param>
    /// <param name="roleId">Role id</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ImportDevelopers(IGuild discordServer, ulong roleId)
    {
        var developers = new List<long>();

        foreach (var user in await discordServer.GetUsersAsync()
                                                .ConfigureAwait(false))
        {
            if (user.RoleIds.Contains(roleId))
            {
                var internalUser = await _userManagementService.GetUserByDiscordAccountId(user)
                                                               .ConfigureAwait(false);

                if (internalUser != null)
                {
                    developers.Add(internalUser.Id);
                }
            }
        }

        await _dbFactory.GetRepository<UserRoleRepository>()
                        .BulkInsertDevelopers(developers)
                        .ConfigureAwait(false);
    }

    #endregion // Methods
}
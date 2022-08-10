using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Exceptions;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Interfaces;
using Scruffy.Services.Raid.DialogElements;

namespace Scruffy.Services.Raid;

/// <summary>
/// Assigning roles to a registration
/// </summary>
public class RaidRoleAssignmentService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Localization service
    /// </summary>
    private readonly LocalizationService _localizationService;

    /// <summary>
    /// Raid roles service
    /// </summary>
    private readonly RaidRolesService _raidRolesService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="raidRolesService">Raid roles service</param>
    public RaidRoleAssignmentService(LocalizationService localizationService, RaidRolesService raidRolesService)
        : base(localizationService)
    {
        _localizationService = localizationService;
        _raidRolesService = raidRolesService;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Assigning roles
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <param name="registrationId">Id of the registration</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task AssignRoles(IContextContainer commandContext, long registrationId)
    {
        var dialogHandler = new DialogHandler(commandContext);
        await using (dialogHandler.ConfigureAwait(false))
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                try
                {
                    dbFactory.GetRepository<RaidRegistrationRoleAssignmentRepository>()
                             .RemoveRange(obj => obj.RegistrationId == registrationId);

                    var roleIds = await dialogHandler.Run<RaidRoleSelectionDialogElement, List<long>>(new RaidRoleSelectionDialogElement(_localizationService, _raidRolesService))
                                                     .ConfigureAwait(false);

                    if (roleIds?.Count > 0)
                    {
                        foreach (var roleId in roleIds)
                        {
                            dbFactory.GetRepository<RaidRegistrationRoleAssignmentRepository>()
                                     .Add(new RaidRegistrationRoleAssignmentEntity
                                          {
                                              RegistrationId = registrationId,
                                              RoleId = roleId
                                          });
                        }
                    }
                }
                catch (ScruffyTimeoutException)
                {
                    await dialogHandler.DeleteMessages()
                                       .ConfigureAwait(false);

                    throw;
                }

                await dialogHandler.DeleteMessages()
                                   .ConfigureAwait(false);
            }
        }
    }

    #endregion // Methods
}
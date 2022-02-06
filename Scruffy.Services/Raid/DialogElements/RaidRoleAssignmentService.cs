using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core.Exceptions;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Assigning roles to a registration
/// </summary>
public class RaidRoleAssignmentService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Localization service
    /// </summary>
    private LocalizationService _localizationService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public RaidRoleAssignmentService(LocalizationService localizationService)
        : base(localizationService)
    {
        _localizationService = localizationService;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Assigning roles
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <param name="registrationId">Id of the registration</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task AssignRoles(CommandContextContainer commandContext, long registrationId)
    {
        var dialogHandler = new DialogHandler(commandContext);
        await using (dialogHandler.ConfigureAwait(false))
        {
            dialogHandler.DialogContext.Messages.Add(commandContext.Message);

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                try
                {
                    dbFactory.GetRepository<RaidRegistrationRoleAssignmentRepository>()
                             .RemoveRange(obj => obj.RegistrationId == registrationId);

                    do
                    {
                        var mainRole = await dialogHandler.Run<RaidRoleSelectionDialogElement, long?>(new RaidRoleSelectionDialogElement(_localizationService, null))
                                                          .ConfigureAwait(false);

                        if (mainRole != null)
                        {
                            var subRole = await dialogHandler.Run<RaidRoleSelectionDialogElement, long?>(new RaidRoleSelectionDialogElement(_localizationService, mainRole))
                                                             .ConfigureAwait(false);

                            dbFactory.GetRepository<RaidRegistrationRoleAssignmentRepository>()
                                     .AddOrRefresh(obj => obj.RegistrationId == registrationId
                                                       && obj.MainRoleId == mainRole.Value
                                                       && obj.SubRoleId == subRole,
                                                   obj =>
                                                   {
                                                       obj.RegistrationId = registrationId;
                                                       obj.MainRoleId = mainRole.Value;
                                                       obj.SubRoleId = subRole;
                                                   });
                        }
                        else
                        {
                            break;
                        }
                    }
                    while (await dialogHandler.Run<RaidRoleSelectionNextDialogElement, bool>(new RaidRoleSelectionNextDialogElement(_localizationService, false))
                                              .ConfigureAwait(false));
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
using System.Threading.Tasks;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;

namespace Scruffy.Services.Raid.DialogElements
{
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
            await using (var dialogHandler = new DialogHandler(commandContext))
            {
                var repeat = true;

                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    while (repeat)
                    {
                        var mainRole = await dialogHandler.Run<RaidRoleSelectionDialogElement, long?>(new RaidRoleSelectionDialogElement(_localizationService, null))
                                                          .ConfigureAwait(false);
                        if (mainRole != null)
                        {
                            var subRole = await dialogHandler.Run<RaidRoleSelectionDialogElement, long?>(new RaidRoleSelectionDialogElement(_localizationService, mainRole))
                                                             .ConfigureAwait(false);

                            dbFactory.GetRepository<RaidRegistrationRoleAssignmentRepository>()
                                     .Add(new RaidRegistrationRoleAssignmentEntity
                                          {
                                              RegistrationId = registrationId,
                                              MainRoleId = mainRole.Value,
                                              SubRoleId = subRole
                                          });
                        }
                        else
                        {
                            repeat = false;
                        }
                    }
                }
            }
        }

        #endregion // Methods
    }
}

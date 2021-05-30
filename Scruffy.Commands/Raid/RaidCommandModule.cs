using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Commands.Base;
using Scruffy.Services.Core;
using Scruffy.Services.CoreData;
using Scruffy.Services.Raid;

namespace Scruffy.Commands.Raid
{
    /// <summary>
    /// Raid commands
    /// </summary>
    [Group("raid")]
    public class RaidCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public RaidCommandModule(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// User management service
        /// </summary>
        public UserManagementService UserManagementService { get; set; }

        /// <summary>
        /// Message builder
        /// </summary>
        public RaidMessageBuilder MessageBuilder { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Starts the setup assistant
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
#if RELEASE
        [Hidden] // TODO
#endif
        public async Task Setup(CommandContext commandContext)
        {
            // TODO
            await Task.Delay(1).ConfigureAwait(false);
        }

        /// <summary>
        /// Joining an appointment
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="name">Name</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("join")]
#if RELEASE
        [Hidden] // TODO
#endif
        public async Task Join(CommandContext commandContext, string name)
        {
            // TODO
            await Task.Delay(1).ConfigureAwait(false);
        }

        /// <summary>
        /// Joining an appointment
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="name">Name</param>
        /// <param name="role">Role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("join")]
#if RELEASE
        [Hidden] // TODO
#endif
        public async Task Join(CommandContext commandContext, string name, string role)
        {
            // TODO
            await Task.Delay(1).ConfigureAwait(false);
        }

        /// <summary>
        /// Leaving an appointment
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="name">Name</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("leave")]
#if RELEASE
        [Hidden] // TODO
#endif
        public async Task Leave(CommandContext commandContext, string name)
        {
            // TODO
            await Task.Delay(1).ConfigureAwait(false);
        }

        /// <summary>
        /// Leaving an appointment
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="name">Name</param>
        /// <param name="role">Role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("leave")]
#if RELEASE
        [Hidden] // TODO
#endif
        public async Task Leave(CommandContext commandContext, string name, string role)
        {
            // TODO
            await Task.Delay(1).ConfigureAwait(false);
        }

        #endregion // Methods

        #region Roles

        /// <summary>
        /// Role administration
        /// </summary>
        [Group("roles")]
        public class RaidRolesCommandModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public RaidRolesCommandModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

            #region Properties

            /// <summary>
            /// User management service
            /// </summary>
            public UserManagementService UserManagementService { get; set; }

            /// <summary>
            /// Message builder
            /// </summary>
            public RaidMessageBuilder MessageBuilder { get; set; }

            /// <summary>
            /// Raid roles service
            /// </summary>
            public RaidRolesService RaidRolesService { get; set; }

            #endregion // Properties

            /// <summary>
            /// Starting the personal roles assistant
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("own")]
#if RELEASE
            [Hidden] // TODO
#endif
            public async Task RolesOwn(CommandContext commandContext)
            {
                // TODO
                await Task.Delay(1).ConfigureAwait(false);
            }

            /// <summary>
            /// Starting the roles assistant
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("setup")]
            public async Task SetupRoles(CommandContext commandContext)
            {
                await RaidRolesService.RunAssistantAsync(commandContext)
                                      .ConfigureAwait(false);
            }
        }

        #endregion // Roles
    }
}

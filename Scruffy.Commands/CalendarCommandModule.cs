using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Services.Calendar;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Commands
{
    /// <summary>
    /// Calendar commands
    /// </summary>
    [Group("calendar")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class CalendarCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public CalendarCommandModule(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Calendar schedules service
        /// </summary>
        public CalendarScheduleService CalendarScheduleService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Adding a one time event
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("add")]
        [RequireAdministratorPermissions]
        public async Task AddOneTimeEvent(CommandContext commandContext)
        {
            await CalendarScheduleService.AddOneTimeEvent(commandContext)
                                         .ConfigureAwait(false);
        }

        #endregion // Methods

        #region Templates

        /// <summary>
        /// Calendar template commands
        /// </summary>
        [Group("templates")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class CalendarTemplateCommandModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public CalendarTemplateCommandModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

            #region Properties

            /// <summary>
            /// Calendar template service
            /// </summary>
            public CalendarTemplateService CalendarTemplateService { get; set; }

            #endregion // Properties

            #region Methods

            /// <summary>
            /// Starting the template assistant
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("setup")]
            [RequireAdministratorPermissions]
            public async Task Setup(CommandContext commandContext)
            {
                await CalendarTemplateService.RunAssistantAsync(commandContext)
                                             .ConfigureAwait(false);
            }

            #endregion // Methods

        }

        #endregion // Templates

        #region Schedule

        /// <summary>
        /// Calendar template commands
        /// </summary>
        [Group("schedules")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class CalendarScheduleCommandModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public CalendarScheduleCommandModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

            #region Properties

            /// <summary>
            /// Calendar schedules service
            /// </summary>
            public CalendarScheduleService CalendarScheduleService { get; set; }

            #endregion // Properties

            #region Methods

            /// <summary>
            /// Starting the schedules assistant
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("setup")]
            [RequireAdministratorPermissions]
            public async Task Setup(CommandContext commandContext)
            {
                await CalendarScheduleService.RunAssistantAsync(commandContext)
                                             .ConfigureAwait(false);
            }

            #endregion // Methods

        }

        #endregion // Schedule

    }
}

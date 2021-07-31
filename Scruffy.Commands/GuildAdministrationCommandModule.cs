using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.GuildAdministration;

namespace Scruffy.Commands
{
    /// <summary>
    /// Administration of the Guild Wars 2 guild
    /// </summary>
    [Group("guild")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class GuildAdministrationCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public GuildAdministrationCommandModule(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Configuration service
        /// </summary>
        public GuildAdministrationConfigurationService ConfigurationService { get; set; }

        /// <summary>
        /// Emblem service
        /// </summary>
        public GuildEmblemService GuildEmblemService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Setup guild administration
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireAdministratorPermissions]
        public async Task Setup(CommandContext commandContext)
        {
            await ConfigurationService.CreateGuildConfiguration(commandContext)
                                      .ConfigureAwait(false);
        }

        /// <summary>
        /// Setting the notification channel
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setNotificationChannel")]
        [RequireAdministratorPermissions]
        public async Task SetNotificationChannel(CommandContext commandContext)
        {
            await ConfigurationService.SetNotificationChannel(commandContext)
                                      .ConfigureAwait(false);
        }

        /// <summary>
        /// Setting up the calendar
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("calendar")]
        [RequireAdministratorPermissions]
        public async Task SetupCalendar(CommandContext commandContext)
        {
            await ConfigurationService.SetupCalendar(commandContext)
                                      .ConfigureAwait(false);
        }

        #endregion // Methods

        #region Emblem

        /// <summary>
        /// Guild emblem
        /// </summary>
        [Group("emblem")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class GuildAdministrationEmblemCommandModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public GuildAdministrationEmblemCommandModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

            #region Properties

            /// <summary>
            /// Emblem service
            /// </summary>
            public GuildEmblemService GuildEmblemService { get; set; }

            #endregion // Properties

            #region Methods

            /// <summary>
            /// Post random guild emblems
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <param name="count">Count</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("random")]
            public async Task SetNotificationChannel(CommandContext commandContext, int count)
            {
                await GuildEmblemService.PostRandomGuildEmblems(commandContext, count)
                                        .ConfigureAwait(false);
            }

            #endregion // Methods
        }

        #endregion Emblem

    }
}

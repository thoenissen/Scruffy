using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Services.Calendar;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.Debug;
using Scruffy.Services.Raid;

namespace Scruffy.Commands
{
    /// <summary>
    /// Debug commands
    /// </summary>
    [Group("debug")]
    [RequireDeveloperPermissions]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class DebugCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public DebugCommandModule(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Dump

        /// <summary>
        /// Listing
        /// </summary>
        [Group("dump")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class DebugDumpModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public DebugDumpModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

            #region Properties

            /// <summary>
            /// Debug-Service
            /// </summary>
            public DebugService DebugService { get; set; }

            #endregion // Properties

            #region Methods

            /// <summary>
            /// List roles
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("text")]
            public async Task Roles(CommandContext commandContext)
            {
                await DebugService.DumpText(commandContext)
                                  .ConfigureAwait(false);
            }

            #endregion // Methods
        }

        #endregion // Dump

        #region Raid

        /// <summary>
        /// Listing
        /// </summary>
        [Group("raid")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class DebugRaidModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public DebugRaidModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

            #region Properties

            /// <summary>
            /// Message builder
            /// </summary>
            public RaidMessageBuilder MessageBuilder { get; set; }

            #endregion // Properties

            #region Methods

            /// <summary>
            /// List roles
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <param name="configurationId">Id of the configuration</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("refresh_message")]
            public async Task Roles(CommandContext commandContext, long configurationId)
            {
                await MessageBuilder.RefreshMessageAsync(configurationId)
                                    .ConfigureAwait(false);

                await commandContext.Message.DeleteAsync()
                                    .ConfigureAwait(false);
            }

            #endregion // Methods
        }

        #endregion // Raid

        #region List

        /// <summary>
        /// Listing
        /// </summary>
        [Group("list")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class DebugListModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public DebugListModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

            #region Properties

            /// <summary>
            /// Debug-Service
            /// </summary>
            public DebugService DebugService { get; set; }

            #endregion // Properties

            #region Methods

            /// <summary>
            /// List roles
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("roles")]
            public async Task Roles(CommandContext commandContext)
            {
                await DebugService.ListRoles(commandContext)
                                  .ConfigureAwait(false);
            }

            /// <summary>
            /// List users
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("users")]
            public async Task Users(CommandContext commandContext)
            {
                await DebugService.ListUsers(commandContext)
                                  .ConfigureAwait(false);
            }

            /// <summary>
            /// List emojis
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("emojis")]
            public async Task Emojis(CommandContext commandContext)
            {
                await DebugService.ListEmojis(commandContext)
                                  .ConfigureAwait(false);
            }

            /// <summary>
            /// List channels
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("channels")]
            public async Task Channels(CommandContext commandContext)
            {
                await DebugService.ListChannels(commandContext)
                                  .ConfigureAwait(false);
            }

            #endregion // Methods
        }

        #endregion // List

        #region Calendar

        /// <summary>
        /// Listing
        /// </summary>
        [Group("calendar")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class DebugCalendarModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public DebugCalendarModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

            #region Properties

            /// <summary>
            /// Message builder
            /// </summary>
            public CalendarMessageBuilderService MessageBuilder { get; set; }

            /// <summary>
            /// Message builder
            /// </summary>
            public CalendarScheduleService ScheduleService { get; set; }

            #endregion // Properties

            #region Methods

            /// <summary>
            /// Refresh calendar message
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("refresh_appointments")]
            public async Task RefreshAppointments(CommandContext commandContext)
            {
                await ScheduleService.CreateAppointments(commandContext.Guild.Id)
                                     .ConfigureAwait(false);

                await commandContext.Message
                                    .DeleteAsync()
                                    .ConfigureAwait(false);
            }

            /// <summary>
            /// Refresh calendar message
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("refresh_message")]
            public async Task RefreshMessage(CommandContext commandContext)
            {
                await MessageBuilder.RefreshMessages(commandContext.Guild.Id)
                                    .ConfigureAwait(false);

                await commandContext.Message
                                    .DeleteAsync()
                                    .ConfigureAwait(false);
            }

            /// <summary>
            /// Refresh motd
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("refresh_motd")]
            public async Task RefreshMotd(CommandContext commandContext)
            {
                await MessageBuilder.RefreshMotds(commandContext.Guild.Id)
                                    .ConfigureAwait(false);

                await commandContext.Message
                                    .DeleteAsync()
                                    .ConfigureAwait(false);
            }

            #endregion // Methods
        }

        #endregion // Raid
    }
}

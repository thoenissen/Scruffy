using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Data.Enumerations.GuildAdministration;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.GuildAdministration;
using Scruffy.Services.GuildAdministration.DialogElements;
using Scruffy.Services.GuildWars2;

namespace Scruffy.Commands
{
    /// <summary>
    /// Administration of the Guild Wars 2 guild
    /// </summary>
    [Group("guild")]
    [Aliases("g")]
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
        [RequireGuild]
        [RequireAdministratorPermissions]
        public Task Setup(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await ConfigurationService.CreateGuildConfiguration(commandContextContainer)
                                                             .ConfigureAwait(false);
                               });
        }

        /// <summary>
        /// Setting the special rank notification channel
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setSpecialRankNotification")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        public Task SetSpecialRankNotification(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await ConfigurationService.SetNotificationChannel(commandContextContainer, GuildChannelConfigurationType.SpecialRankRankChange)
                                                             .ConfigureAwait(false);
                               });
        }

        /// <summary>
        /// Setting the calendar notification channel
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setCalendarReminderNotification")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        public Task SetCalendarReminderNotification(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await ConfigurationService.SetNotificationChannel(commandContextContainer, GuildChannelConfigurationType.CalendarReminder)
                                                             .ConfigureAwait(false);
                               });
        }

        /// <summary>
        /// Setting the guild log notification
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setGuildLogNotification")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        public Task SetGuildLogNotification(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await ConfigurationService.SetNotificationChannel(commandContextContainer, GuildChannelConfigurationType.GuildLogNotification)
                                                             .ConfigureAwait(false);
                               });
        }

        /// <summary>
        /// Setting up the motd builder
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("motd")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        public Task SetupMotd(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await ConfigurationService.SetupMotd(commandContextContainer)
                                                             .ConfigureAwait(false);
                               });
        }

        /// <summary>
        /// Setting up the calendar
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("calendar")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        public Task SetupCalendar(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await ConfigurationService.SetupCalendar(commandContextContainer)
                                                             .ConfigureAwait(false);
                               });
        }

        #endregion // Methods

        #region Emblem

        /// <summary>
        /// Guild emblem
        /// </summary>
        [Group("emblem")]
        [Aliases("e")]
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
            [RequireGuild]
            public Task SetNotificationChannel(CommandContext commandContext, int count)
            {
                return InvokeAsync(commandContext,
                                   async commandContextContainer =>
                                   {
                                       await GuildEmblemService.PostRandomGuildEmblems(commandContextContainer, count)
                                                               .ConfigureAwait(false);
                                   });
            }

            #endregion // Methods
        }

        #endregion Emblem

        #region Bank

        /// <summary>
        /// Guild bank
        /// </summary>
        [Group("bank")]
        [Aliases("b")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class GuildAdministrationBankCommandModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public GuildAdministrationBankCommandModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

            #region Properties

            /// <summary>
            /// Bank service
            /// </summary>
            public GuildBankService BankService { get; set; }

            #endregion // Properties

            #region Methods

            /// <summary>
            /// Check the guild bank
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("check")]
            [RequireGuild]
            public Task SetNotificationChannel(CommandContext commandContext)
            {
                return InvokeAsync(commandContext,
                                   async commandContextContainer =>
                                   {
                                       await BankService.Check(commandContextContainer)
                                                        .ConfigureAwait(false);
                                   });
            }

            #endregion // Methods
        }

        #endregion Emblem

        #region Special ranks

        /// <summary>
        /// Special ranks
        /// </summary>
        [Group("specialrank")]
        [Aliases("s")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class GuildAdministrationSpecialRankCommandModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public GuildAdministrationSpecialRankCommandModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

            /// <summary>
            /// Special rank service
            /// </summary>
            public GuildSpecialRankService SpecialRankService { get; set; }

            #region Methods

            /// <summary>
            /// Special ranks configuration
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("setup")]
            [RequireGuild]
            public Task Setup(CommandContext commandContext)
            {
                return InvokeAsync(commandContext,
                                   async commandContextContainer =>
                                   {
                                       bool repeat;

                                       do
                                       {
                                           repeat = await DialogHandler.Run<GuildAdministrationSpecialRankSetupDialogElement, bool>(commandContextContainer).ConfigureAwait(false);
                                       }
                                       while (repeat);
                                   });
            }

            /// <summary>
            /// Special ranks configuration
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("overview")]
            [RequireGuild]
            public Task Overview(CommandContext commandContext)
            {
                return InvokeAsync(commandContext,
                                   async commandContextContainer =>
                                   {
                                       await SpecialRankService.PostOverview(commandContextContainer)
                                                               .ConfigureAwait(false);
                                   });
            }

            #endregion // Methods
        }

        #endregion // Special ranks

        #region Charts

        /// <summary>
        /// Guild bank
        /// </summary>
        [Group("charts")]
        [Aliases("c")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class GuildAdministrationChartCommandModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public GuildAdministrationChartCommandModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

            #region Properties

            /// <summary>
            /// Bank service
            /// </summary>
            public WorldsService WorldsService { get; set; }

            #endregion // Properties

            #region Methods

            /// <summary>
            /// Worlds overview
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("worlds")]
            [RequireGuild]
            public Task SetNotificationChannel(CommandContext commandContext)
            {
                return InvokeAsync(commandContext,
                                   async commandContextContainer =>
                                   {
                                       await WorldsService.PostWorldsOverview(commandContextContainer)
                                                          .ConfigureAwait(false);
                                   });
            }

            #endregion // Methods
        }

        #endregion Emblem
    }
}

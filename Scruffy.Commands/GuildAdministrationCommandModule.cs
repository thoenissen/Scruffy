using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.GuildAdministration;
using Scruffy.Services.GuildAdministration.DialogElements;

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
        /// Setting the notification channel
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setNotificationChannel")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        public Task SetNotificationChannel(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await ConfigurationService.SetNotificationChannel(commandContextContainer)
                                                             .ConfigureAwait(false);
                               });
        }

        /// <summary>
        /// Setting the reminder channel
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setReminderChannel")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        public Task SetReminderChannel(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await ConfigurationService.SetReminderChannel(commandContextContainer)
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

    }
}

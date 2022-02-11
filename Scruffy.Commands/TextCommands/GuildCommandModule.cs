using System.Globalization;

using Discord.Commands;

using Scruffy.Data.Enumerations.Guild;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;
using Scruffy.Services.Guild;
using Scruffy.Services.Guild.DialogElements;
using Scruffy.Services.GuildWars2;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Administration of the Guild Wars 2 guild
/// </summary>
[Group("guild")]
[Alias("g")]
[BlockedChannelCheck]
[HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
public class GuildCommandModule : LocatedTextCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Configuration service
    /// </summary>
    public GuildConfigurationService ConfigurationService { get; set; }

    /// <summary>
    /// Emblem service
    /// </summary>
    public GuildEmblemService GuildEmblemService { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Setup guild administration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setup")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public async Task Setup()
    {
        await ConfigurationService.CreateGuildConfiguration(Context)
                                  .ConfigureAwait(false);
    }

    /// <summary>
    /// Setting the special rank notification channel
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setSpecialRankNotification")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public async Task SetSpecialRankNotification()
    {
        await ConfigurationService.SetNotificationChannel(Context, GuildChannelConfigurationType.SpecialRankRankChange)
                                  .ConfigureAwait(false);
    }

    /// <summary>
    /// Setting the calendar notification channel
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setCalendarReminderNotification")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public async Task SetCalendarReminderNotification()
    {
        await ConfigurationService.SetNotificationChannel(Context, GuildChannelConfigurationType.CalendarReminder)
                                  .ConfigureAwait(false);
    }

    /// <summary>
    /// Setting the guild log notification
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setGuildLogNotification")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public async Task SetGuildLogNotification()
    {
        await ConfigurationService.SetNotificationChannel(Context, GuildChannelConfigurationType.GuildLogNotification)
                                  .ConfigureAwait(false);
    }

    /// <summary>
    /// Setting up the motd builder
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("motd")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public async Task SetupMotd()
    {
        await ConfigurationService.SetupMotd(Context)
                                  .ConfigureAwait(false);
    }

    /// <summary>
    /// Setting up the calendar
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("calendar")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public async Task SetupCalendar()
    {
        await ConfigurationService.SetupCalendar(Context)
                                  .ConfigureAwait(false);
    }

    #endregion // Methods

    #region Emblem

    /// <summary>
    /// Guild emblem
    /// </summary>
    [Group("emblem")]
    [Alias("e")]
    public class GuildEmblemCommandModule : LocatedTextCommandModuleBase
    {
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
        /// <param name="count">Count</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("random")]
        [RequireContext(ContextType.Guild)]
        public async Task SetNotificationChannel(int count)
        {
            await GuildEmblemService.PostRandomGuildEmblems(Context, count)
                                    .ConfigureAwait(false);
        }

        #endregion // Methods
    }

    #endregion Emblem

    #region Bank

    /// <summary>
    /// Guild bank
    /// </summary>
    [Group("bank")]
    [Alias("b")]
    [RequireContext(ContextType.Guild)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public class GuildBankCommandModule : LocatedTextCommandModuleBase
    {
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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("check")]
        [RequireContext(ContextType.Guild)]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public async Task SetNotificationChannel()
        {
            await BankService.Check(Context)
                             .ConfigureAwait(false);
        }

        #endregion // Methods

        #region Unlocks

        /// <summary>
        /// Guild bank
        /// </summary>
        [Group("unlocks")]
        [Alias("u")]
        [RequireContext(ContextType.Guild)]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public class GuildBankUnlocksCommandModule : LocatedTextCommandModuleBase
        {
            #region Constructor

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
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("dyes")]
            [RequireContext(ContextType.Guild)]
            [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
            public async Task SetNotificationChannel()
            {
                await BankService.CheckUnlocksDyes(Context)
                                 .ConfigureAwait(false);
            }

            #endregion // Methods
        }

        #endregion
    }

    #endregion // Bank

    #region Ranks

    /// <summary>
    /// Special ranks
    /// </summary>
    [Group("rank")]
    [Alias("r")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public class GuildRankCommandModule : LocatedTextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Special ranks configuration
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public async Task Setup()
        {
            bool repeat;

            do
            {
                repeat = await DialogHandler.Run<GuildRankSetupDialogElement, bool>(Context)
                                            .ConfigureAwait(false);
            }
            while (repeat);
        }

        #endregion // Methods
    }

    #endregion // Ranks

    #region Special ranks

    /// <summary>
    /// Special ranks
    /// </summary>
    [Group("specialrank")]
    [Alias("s")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public class GuildSpecialRankCommandModule : LocatedTextCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Special rank service
        /// </summary>
        public GuildSpecialRankService SpecialRankService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Special ranks configuration
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public async Task Setup()
        {
            bool repeat;

            do
            {
                repeat = await DialogHandler.Run<GuildSpecialRankSetupDialogElement, bool>(Context)
                                            .ConfigureAwait(false);
            }
            while (repeat);
        }

        /// <summary>
        /// Special ranks configuration
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("overview")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public async Task Overview()
        {
            await SpecialRankService.PostOverview(Context)
                                    .ConfigureAwait(false);
        }

        #endregion // Methods
    }

    #endregion // Special ranks

    #region Charts

    /// <summary>
    /// Guild bank
    /// </summary>
    [Group("charts")]
    [Alias("c")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public class GuildChartCommandModule : LocatedTextCommandModuleBase
    {
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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("worlds")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public async Task SetNotificationChannel()
        {
            await WorldsService.PostWorldsOverview(Context)
                               .ConfigureAwait(false);
        }

        #endregion // Methods
    }

    #endregion Charts

    #region Exports

    /// <summary>
    /// Exporting data
    /// </summary>
    [Group("export")]
    [Alias("e")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public class GuildExportCommandModule : LocatedTextCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Export service
        /// </summary>
        public GuildExportService GuildExportService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Worlds overview
        /// </summary>
        /// <param name="mode">Mode</param>
        /// <param name="sinceDate">Since Date</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("stash")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public async Task ExportStashLog(string mode, string sinceDate)
        {
            if (DateTime.TryParseExact(sinceDate,
                                       "yyyy-MM-dd",
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.None,
                                       out var date))
            {
                if (mode == "sum")
                {
                    await GuildExportService.ExportStashLogSummarized(Context, date)
                                            .ConfigureAwait(false);
                }
                else
                {
                    await GuildExportService.ExportStashLog(Context, date)
                                            .ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Worlds overview
        /// </summary>
        /// <param name="mode">Mode</param>
        /// <param name="sinceDate">Since Date</param>
        /// <param name="sinceTime">Since Time</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("stash")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public async Task ExportStashLog(string mode, string sinceDate, string sinceTime)
        {
            if (DateTime.TryParseExact(sinceDate,
                                       "yyyy-MM-dd",
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.None,
                                       out var date)
             && TimeSpan.TryParseExact(sinceTime, "hh\\:mm", null, out var time))
            {
                date = date.Add(time);

                if (mode == "sum")
                {
                    await GuildExportService.ExportStashLogSummarized(Context, date)
                                            .ConfigureAwait(false);
                }
                else
                {
                    await GuildExportService.ExportStashLog(Context, date)
                                            .ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Worlds overview
        /// </summary>
        /// <param name="mode">Mode</param>
        /// <param name="sinceDate">Since Date</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("upgrades")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public async Task ExportUpgradesLog(string mode, string sinceDate)
        {
            if (DateTime.TryParseExact(sinceDate,
                                       "yyyy-MM-dd",
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.None,
                                       out var date))
            {
                if (mode == "sum")
                {
                    await GuildExportService.ExportUpgradesLogSummarized(Context, date)
                                            .ConfigureAwait(false);
                }
                else
                {
                    await GuildExportService.ExportUpgradesLog(Context, date)
                                            .ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Worlds overview
        /// </summary>
        /// <param name="mode">Mode</param>
        /// <param name="sinceDate">Since Date</param>
        /// <param name="sinceTime">Since Time</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("upgrades")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public async Task ExportUpgradesLog(string mode, string sinceDate, string sinceTime)
        {
            if (DateTime.TryParseExact(sinceDate,
                                       "yyyy-MM-dd",
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.None,
                                       out var date)
             && TimeSpan.TryParseExact(sinceTime, "hh\\:mm", null, out var time))
            {
                date = date.Add(time);

                if (mode == "sum")
                {
                    await GuildExportService.ExportUpgradesLogSummarized(Context, date)
                                            .ConfigureAwait(false);
                }
                else
                {
                    await GuildExportService.ExportUpgradesLog(Context, date)
                                            .ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Login Activity
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("activity")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public async Task ExportUpgradesLog()
        {
            await GuildExportService.ExportLoginActivityLog(Context)
                                    .ConfigureAwait(false);
        }

        /// <summary>
        /// Representation state
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("representation")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public async Task ExportRepresentation()
        {
            await GuildExportService.ExportRepresentation(Context)
                                    .ConfigureAwait(false);
        }

        /// <summary>
        /// Members
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("members")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public async Task ExportGuildMembers()
        {
            await GuildExportService.ExportGuildMembers(Context)
                                    .ConfigureAwait(false);
        }

        /// <summary>
        /// Roles
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("roles")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public async Task ExportGuildRoles()
        {
            await GuildExportService.ExportGuildRoles(Context)
                                    .ConfigureAwait(false);
        }

        /// <summary>
        /// Current rank points
        /// </summary>
        /// <param name="sinceDate">Since date</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("points")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public async Task ExportCurrentRankPoints(string sinceDate)
        {
            if (DateTime.TryParseExact(sinceDate,
                                       "yyyy-MM-dd",
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.None,
                                       out var date))
            {
                await GuildExportService.ExportGuildRankPoints(Context, date)
                                        .ConfigureAwait(false);
            }
        }

        #endregion // Methods
    }

    #endregion Exports
}
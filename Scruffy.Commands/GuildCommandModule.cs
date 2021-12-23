using System.Globalization;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Data.Enumerations.Guild;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.Guild;
using Scruffy.Services.Guild.DialogElements;
using Scruffy.Services.GuildWars2;

namespace Scruffy.Commands;

/// <summary>
/// Administration of the Guild Wars 2 guild
/// </summary>
[Group("guild")]
[Aliases("g")]
[ModuleLifespan(ModuleLifespan.Transient)]
[HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
public class GuildCommandModule : LocatedCommandModuleBase
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
    public class GuildEmblemCommandModule : LocatedCommandModuleBase
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
    [RequireGuild]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public class GuildBankCommandModule : LocatedCommandModuleBase
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
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("check")]
        [RequireGuild]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
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

        #region Unlocks

        /// <summary>
        /// Guild bank
        /// </summary>
        [Group("unlocks")]
        [Aliases("u")]
        [RequireGuild]
        [ModuleLifespan(ModuleLifespan.Transient)]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public class GuildBankUnlocksCommandModule : LocatedCommandModuleBase
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
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("dyes")]
            [RequireGuild]
            [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
            public Task SetNotificationChannel(CommandContext commandContext)
            {
                return InvokeAsync(commandContext,
                                   async commandContextContainer =>
                                   {
                                       await BankService.CheckUnlocksDyes(commandContextContainer)
                                                        .ConfigureAwait(false);
                                   });
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
    [Aliases("r")]
    [RequireGuild]
    [RequireAdministratorPermissions]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public class GuildRankCommandModule : LocatedCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Special ranks configuration
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
                                   bool repeat;

                                   do
                                   {
                                       repeat = await DialogHandler.Run<GuildRankSetupDialogElement, bool>(commandContextContainer).ConfigureAwait(false);
                                   }
                                   while (repeat);
                               });
        }

        #endregion // Methods
    }

    #endregion // Ranks

    #region Special ranks

    /// <summary>
    /// Special ranks
    /// </summary>
    [Group("specialrank")]
    [Aliases("s")]
    [RequireGuild]
    [RequireAdministratorPermissions]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public class GuildSpecialRankCommandModule : LocatedCommandModuleBase
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
                                   bool repeat;

                                   do
                                   {
                                       repeat = await DialogHandler.Run<GuildSpecialRankSetupDialogElement, bool>(commandContextContainer).ConfigureAwait(false);
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
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
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
    [RequireGuild]
    [RequireAdministratorPermissions]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public class GuildChartCommandModule : LocatedCommandModuleBase
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
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("worlds")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
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

    #endregion Charts

    #region Exports

    /// <summary>
    /// Exporting data
    /// </summary>
    [Group("export")]
    [Aliases("e")]
    [RequireGuild]
    [RequireAdministratorPermissions]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public class GuildExportCommandModule : LocatedCommandModuleBase
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
        /// <param name="commandContext">Current command context</param>
        /// <param name="mode">Mode</param>
        /// <param name="sinceDate">Since Date</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("stash")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public Task ExportStashLog(CommandContext commandContext, string mode, string sinceDate)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   if (DateTime.TryParseExact(sinceDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                                   {
                                       if (mode == "sum")
                                       {
                                           await GuildExportService.ExportStashLogSummarized(commandContextContainer, date)
                                                                   .ConfigureAwait(false);
                                       }
                                       else
                                       {
                                           await GuildExportService.ExportStashLog(commandContextContainer, date)
                                                                   .ConfigureAwait(false);
                                       }
                                   }
                               });
        }

        /// <summary>
        /// Worlds overview
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="mode">Mode</param>
        /// <param name="sinceDate">Since Date</param>
        /// <param name="sinceTime">Since Time</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("stash")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public Task ExportStashLog(CommandContext commandContext, string mode, string sinceDate, string sinceTime)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   if (DateTime.TryParseExact(sinceDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                                    && TimeSpan.TryParseExact(sinceTime, "hh\\:mm", null, out var time))
                                   {
                                       date = date.Add(time);

                                       if (mode == "sum")
                                       {
                                           await GuildExportService.ExportStashLogSummarized(commandContextContainer, date)
                                                                   .ConfigureAwait(false);
                                       }
                                       else
                                       {
                                           await GuildExportService.ExportStashLog(commandContextContainer, date)
                                                                   .ConfigureAwait(false);
                                       }
                                   }
                               });
        }

        /// <summary>
        /// Worlds overview
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="mode">Mode</param>
        /// <param name="sinceDate">Since Date</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("upgrades")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public Task ExportUpgradesLog(CommandContext commandContext, string mode, string sinceDate)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   if (DateTime.TryParseExact(sinceDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                                   {
                                       if (mode == "sum")
                                       {
                                           await GuildExportService.ExportUpgradesLogSummarized(commandContextContainer, date)
                                                                   .ConfigureAwait(false);
                                       }
                                       else
                                       {
                                           await GuildExportService.ExportUpgradesLog(commandContextContainer, date)
                                                                   .ConfigureAwait(false);
                                       }
                                   }
                               });
        }

        /// <summary>
        /// Worlds overview
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="mode">Mode</param>
        /// <param name="sinceDate">Since Date</param>
        /// <param name="sinceTime">Since Time</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("upgrades")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public Task ExportUpgradesLog(CommandContext commandContext, string mode, string sinceDate, string sinceTime)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   if (DateTime.TryParseExact(sinceDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                                    && TimeSpan.TryParseExact(sinceTime, "hh\\:mm", null, out var time))
                                   {
                                       date = date.Add(time);

                                       if (mode == "sum")
                                       {
                                           await GuildExportService.ExportUpgradesLogSummarized(commandContextContainer, date)
                                                                   .ConfigureAwait(false);
                                       }
                                       else
                                       {
                                           await GuildExportService.ExportUpgradesLog(commandContextContainer, date)
                                                                   .ConfigureAwait(false);
                                       }
                                   }
                               });
        }

        /// <summary>
        /// Login Activity
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("activity")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public Task ExportUpgradesLog(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await GuildExportService.ExportLoginActivityLog(commandContextContainer)
                                                           .ConfigureAwait(false);
                               });
        }

        /// <summary>
        /// Representation state
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("representation")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public Task ExportRepresentation(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await GuildExportService.ExportRepresentation(commandContextContainer)
                                                           .ConfigureAwait(false);
                               });
        }

        /// <summary>
        /// Members
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("members")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public Task ExportGuildMembers(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await GuildExportService.ExportGuildMembers(commandContextContainer)
                                                           .ConfigureAwait(false);
                               });
        }

        #endregion // Methods
    }

    #endregion Exports
}
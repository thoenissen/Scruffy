using Discord;
using Discord.Commands;

using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Administration of the Guild Wars 2 guild
/// </summary>
[Group("guild")]
[Alias("g")]
[BlockedChannelCheck]
public class GuildCommandModule : LocatedTextCommandModuleBase
{
    #region Methods

    /// <summary>
    /// Setup guild administration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setup")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task Setup() => ShowMigrationMessage("guild-admin configuration");

    /// <summary>
    /// Setting the special rank notification channel
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setSpecialRankNotification")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task SetSpecialRankNotification() => ShowMigrationMessage("guild-admin configuration");

    /// <summary>
    /// Setting the calendar notification channel
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setCalendarReminderNotification")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task SetCalendarReminderNotification() => ShowMigrationMessage("guild-admin configuration");

    /// <summary>
    /// Setting the guild log notification
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setGuildLogNotification")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task SetGuildLogNotification() => ShowMigrationMessage("guild-admin configuration");

    /// <summary>
    /// Setting the guild log notification
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setRankNotification")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task SetRankNotification() => ShowMigrationMessage("guild-admin configuration");

    /// <summary>
    /// Setting up the motd builder
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("motd")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task SetupMotd() => ShowMigrationMessage("guild-admin configuration");

    /// <summary>
    /// Setting up the calendar
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("calendar")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task SetupCalendar() => ShowMigrationMessage("guild-admin configuration");

    #endregion // Methods

    #region Emblem

    /// <summary>
    /// Guild emblem
    /// </summary>
    [Group("emblem")]
    [Alias("e")]
    public class GuildEmblemCommandModule : LocatedTextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Post random guild emblems
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("random")]
        [RequireContext(ContextType.Guild)]
        public Task SetNotificationChannel(int count) => ShowMigrationMessage("guild random-emblem");

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
    public class GuildBankCommandModule : LocatedTextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Check the guild bank
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("check")]
        [RequireContext(ContextType.Guild)]
        public Task SetNotificationChannel() => ShowMigrationMessage("guild bank-check");

        #endregion // Methods

        #region Unlocks

        /// <summary>
        /// Guild bank
        /// </summary>
        [Group("unlocks")]
        [Alias("u")]
        [RequireContext(ContextType.Guild)]
        public class GuildBankUnlocksCommandModule : LocatedTextCommandModuleBase
        {
            #region Methods

            /// <summary>
            /// Check the guild bank
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("dyes")]
            [RequireContext(ContextType.Guild)]
            public Task SetNotificationChannel() => ShowMigrationMessage("guild bank-unlocks");

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
        public Task Setup() => ShowMigrationMessage("guild-admin configuration");

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
    public class GuildSpecialRankCommandModule : LocatedTextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Special ranks configuration
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task Setup() => ShowMigrationMessage("guild-admin configuration");

        /// <summary>
        /// Special ranks configuration
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("overview")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task Overview() => ShowMigrationMessage("guild-admin overview");

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
    public class GuildChartCommandModule : LocatedTextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Worlds overview
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("worlds")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task SetNotificationChannel() => ShowMigrationMessage("guild-admin overviews");

        #endregion // Methods
    }

    #endregion Charts

    #region Activity

    /// <summary>
    /// Guild bank
    /// </summary>
    [Group("activity")]
    [Alias("a")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public class GuildActivityCommandModule : LocatedTextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Voice roles configuration
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("voice")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task VoiceConfiguration() => ShowMigrationMessage("guild-admin configuration");

        /// <summary>
        /// Message roles configuration
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("message")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task MessageConfiguration() => ShowMigrationMessage("guild-admin configuration");

        #endregion // Methods
    }

    #endregion Activity

    #region Exports

    /// <summary>
    /// Exporting data
    /// </summary>
    [Group("export")]
    [Alias("e")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public class GuildExportCommandModule : LocatedTextCommandModuleBase
    {
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
        public Task ExportStashLog(string mode, string sinceDate) => ShowMigrationMessage("guild-admin export");

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
        public Task ExportStashLog(string mode, string sinceDate, string sinceTime) => ShowMigrationMessage("guild-admin export");

        /// <summary>
        /// Worlds overview
        /// </summary>
        /// <param name="mode">Mode</param>
        /// <param name="sinceDate">Since Date</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("upgrades")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task ExportUpgradesLog(string mode, string sinceDate) => ShowMigrationMessage("guild-admin export");

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
        public Task ExportUpgradesLog(string mode, string sinceDate, string sinceTime) => ShowMigrationMessage("guild-admin export");

        /// <summary>
        /// Login Activity
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("activity")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task ExportUpgradesLog() => ShowMigrationMessage("guild-admin export");

        /// <summary>
        /// Representation state
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("representation")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task ExportRepresentation() => ShowMigrationMessage("guild-admin export");

        /// <summary>
        /// Members
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("members")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task ExportGuildMembers() => ShowMigrationMessage("guild-admin export");

        /// <summary>
        /// Roles
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("roles")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task ExportGuildRoles() => ShowMigrationMessage("guild-admin export");

        /// <summary>
        /// Current rank points
        /// </summary>
        /// <param name="sinceDate">Since date</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("points")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task ExportCurrentRankPoints(string sinceDate) => ShowMigrationMessage("guild-admin export");

        /// <summary>
        /// Custom values
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("items")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task ExportCustomValues() => ShowMigrationMessage("guild-admin export");

        /// <summary>
        /// Current rank assignments
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("assignments")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task ExportCurrentRankAssignments() => ShowMigrationMessage("guild-admin export");

        #endregion // Methods
    }

    #endregion Exports

    #region Configuration

    /// <summary>
    /// Guild bank
    /// </summary>
    [Group("configuration")]
    [Alias("c", "config")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public class GuildConfigurationCommandModule : LocatedTextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Item configuration
        /// </summary>
        /// <param name="id">Item id</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("item")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task StartItemsConfiguration(int id) => ShowMigrationMessage("guild-admin configuration");

        /// <summary>
        /// User configuration
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("user")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task StartUserConfiguration(IGuildUser user) => ShowMigrationMessage("guild-admin configuration");

        #endregion // Methods
    }

    #endregion Activity

    #region Ranking

    /// <summary>
    /// Guild bank
    /// </summary>
    [Group("ranking")]
    [RequireContext(ContextType.Guild)]
    public class GuildRankingCommandModule : LocatedTextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// General overview
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("overview")]
        [RequireAdministratorPermissions]
        [RequireContext(ContextType.Guild)]
        public Task PostOverview() => ShowMigrationMessage("guild-admin overview");

        /// <summary>
        /// Personal overview
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("me")]
        [RequireContext(ContextType.Guild)]
        public Task PostPersonalOverview() => ShowMigrationMessage("guild ranking-me");

        /// <summary>
        /// Personal overview
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command]
        [RequireAdministratorPermissions]
        [RequireContext(ContextType.Guild)]
        public Task PostPersonalOverview(IGuildUser user) => ShowMigrationMessage("guild-admin ranking-of");

        /// <summary>
        /// Check assignments
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("check")]
        [RequireAdministratorPermissions]
        [RequireContext(ContextType.Guild)]
        public Task PostAssignmentOverview() => ShowMigrationMessage("guild-admin check");

        #endregion // Methods
    }

    #endregion Ranking
}
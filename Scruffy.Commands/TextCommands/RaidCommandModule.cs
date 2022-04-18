using Discord;
using Discord.Commands;

using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Raid commands
/// </summary>
[Group("raid")]
[Alias("ra")]
[BlockedChannelCheck]
public class RaidCommandModule : LocatedTextCommandModuleBase
{
    #region Methods

    /// <summary>
    /// Starts the setup assistant
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setup")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task Setup() => ShowMigrationMessage("raid-admin setup");

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("join")]
    [RequireContext(ContextType.Guild)]
    public Task Join(string name) => ShowMigrationMessage("raid join");

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("joinUser")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task Join(IGuildUser user, string name) => ShowMigrationMessage("raid-admin join-user");

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("leave")]
    [RequireContext(ContextType.Guild)]
    public Task Leave(string name) => ShowMigrationMessage("raid leave");

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("leaveUser")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task Leave(IGuildUser user, string name) => ShowMigrationMessage("raid-admin leave-user");

    /// <summary>
    /// Commiting the current raid appointment
    /// </summary>
    /// <param name="name">Alias name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setTemplate")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task SetTemplate(string name) => ShowMigrationMessage("raid-admin set-template");

    /// <summary>
    /// Commiting the current raid appointment
    /// </summary>
    /// <param name="name">Alias name</param>
    /// <param name="count">Count</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setGroupCount")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task SetGroupCount(string name, int count) => ShowMigrationMessage("raid-admin set-group-count");

    /// <summary>
    /// Commiting the current raid appointment
    /// </summary>
    /// <param name="aliasName">Alias name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("commit")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task Commit(string aliasName) => ShowMigrationMessage("raid-admin commit");

    /// <summary>
    /// Daily logs
    /// </summary>
    /// <param name="day">Day</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("logs")]
    public Task Logs(string day = null) => ShowMigrationMessage("raid logs");

    /// <summary>
    /// Post guides overview
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("guides")]
    public Task Guides() => ShowMigrationMessage("raid guides");

    #endregion // Methods

    #region Roles

    /// <summary>
    /// Role administration
    /// </summary>
    [Group("roles")]
    [Alias("r")]
    public class RaidRolesCommandModule : LocatedTextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Starting the roles assistant
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task SetupRoles() => ShowMigrationMessage("raid-admin configuration");

        #endregion // Methods
    }

    #endregion // Roles

    #region Templates

    /// <summary>
    /// Template administration
    /// </summary>
    [Group("templates")]
    [Alias("t")]
    public class RaidTemplatesCommandModule : LocatedTextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Starting the templates assistant
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task Setup() => ShowMigrationMessage("raid-admin configuration");

        #endregion // Methods
    }

    #endregion // Templates

    #region Levels

    /// <summary>
    /// Template administration
    /// </summary>
    [Group("levels")]
    [Alias("l", "level")]
    public class RaidExperienceLevelsCommandModule : LocatedTextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Starting the experience levels assistant
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task Setup() => ShowMigrationMessage("raid-admin configuration");

        /// <summary>
        /// Set experience levels to players
        /// </summary>
        /// <param name="aliasName">Alias name</param>
        /// <param name="discordUsers">Users</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("set")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task SetExperienceLevel(string aliasName, params IGuildUser[] discordUsers) => ShowMigrationMessage("raid-admin set-experience-level");

        #endregion // Methods
    }

    #endregion // Templates

    #region Overview

    /// <summary>
    /// Overviews
    /// </summary>
    [Group("overview")]
    [Alias("o")]
    public class RaidOverviewCommandModule : LocatedTextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Post overview of participation points
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("participation")]
        [RequireContext(ContextType.Guild)]
        public Task PostParticipationOverview() => ShowMigrationMessage("raid-admin overview");

        /// <summary>
        /// Post overview of experience roles
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("levels")]
        [RequireContext(ContextType.Guild)]
        public Task PostExperienceLevelOverview() => ShowMigrationMessage("raid-admin overview");

        #endregion // Methods
    }

    #endregion // Overview
}
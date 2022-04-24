using Discord.Commands;

using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Calendar commands
/// </summary>
[Group("calendar")]
[Alias("ca")]
[RequireContext(ContextType.Guild)]
[RequireAdministratorPermissions]
[BlockedChannelCheck]
public class CalendarCommandModule : LocatedTextCommandModuleBase
{
    #region Methods

    /// <summary>
    /// Adding a one time event
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("add")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task AddOneTimeEvent() => ShowMigrationMessage("calendar-admin configuration");

    /// <summary>
    /// Editing participants
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("participants")]
    [Alias("p")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task EditParticipants() => ShowMigrationMessage("calendar-admin participants");

    #endregion // Methods

    #region Templates

    /// <summary>
    /// Calendar template commands
    /// </summary>
    [Group("templates")]
    [Alias("t")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public class CalendarTemplateCommandModule : LocatedTextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Starting the template assistant
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task Setup() => ShowMigrationMessage("calendar-admin configuration");

        #endregion // Methods

    }

    #endregion // Templates

    #region Schedule

    /// <summary>
    /// Calendar template commands
    /// </summary>
    [Group("schedules")]
    [Alias("s")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public class CalendarScheduleCommandModule : LocatedTextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Starting the schedules assistant
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task Setup() => ShowMigrationMessage("calendar-admin configuration");

        #endregion // Methods

    }

    #endregion // Schedule
}
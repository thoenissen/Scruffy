using Discord;
using Discord.Commands;

using Scruffy.Services.Administration;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Admin commands
/// </summary>
[Group("admin")]
[Alias("ad")]
[RequireContext(ContextType.Guild)]
[RequireAdministratorPermissions]
public class AdministrationCommandModule : LocatedTextCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Configuration service
    /// </summary>
    public AdministrationCommandHandler AdministrationService { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Rename user
    /// </summary>
    /// <param name="member">Member</param>
    /// <param name="name">name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("rename")]
    [RequireAdministratorPermissions]
    public Task Rename(IGuildUser member, [Remainder] string name) => ShowMigrationMessage("admin rename-member");

    /// <summary>
    /// Rename user
    /// </summary>
    /// <param name="role">Member</param>
    /// <param name="name">name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("rename")]
    [RequireAdministratorPermissions]
    public Task Rename(IRole role, [Remainder] string name) => ShowMigrationMessage("admin rename-role");

    #endregion // Methods

    #region Channel

    /// <summary>
    /// Channel commands
    /// </summary>
    [Group("channel")]
    [Alias("c")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public class AdministrationChannelCommandModule : LocatedTextCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Blocked channels
        /// </summary>
        public BlockedChannelService BlockedChannelService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Block channel
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("block")]
        [RequireAdministratorPermissions]
        public Task BlockChannel() => ShowMigrationMessage("admin channel-configuration");

        /// <summary>
        /// Block channel
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("unblock")]
        [RequireAdministratorPermissions]
        public Task UnblockChannel() => ShowMigrationMessage("admin channel-configuration");

        #endregion // Methods
    }

    #endregion // Channel

    #region SlashCommands

    /// <summary>
    /// SlashCommands commands
    /// </summary>
    [Group("slashCommands")]
    [Alias("s")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public class AdministrationSlashCommandsCommandModule : LocatedTextCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Interaction service
        /// </summary>
        public Discord.Interactions.InteractionService InteractionService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Install SlashCommands
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("install")]
        [RequireAdministratorPermissions]
        public Task InstallSlashCommands() => ShowMigrationMessage("configuration");

        /// <summary>
        /// Uninstall SlashCommands
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("uninstall")]
        [RequireAdministratorPermissions]
        public Task UninstallSlashCommands() => ShowMigrationMessage("configuration");

        /// <summary>
        /// Set SlashCommand permissions
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="role">Role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("set-permissions")]
        [RequireAdministratorPermissions]
        public Task SetSlashCommandsPermissions(string groupName, IRole role) => ShowMigrationMessage("configuration");

        /// <summary>
        /// Remove SlashCommands permissions
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="role">Role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("remove-permissions")]
        [RequireAdministratorPermissions]
        public Task RemoveSlashCommandsPermissions(string groupName, IRole role) => ShowMigrationMessage("configuration");

        #endregion // Methods
    }

    #endregion // SlashCommands

}
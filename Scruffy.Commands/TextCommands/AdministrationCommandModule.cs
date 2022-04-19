using System.Reflection;

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
        public async Task BlockChannel()
        {
            if (Context.Channel is IGuildChannel guildChannel)
            {
                BlockedChannelService.AddChannel(guildChannel);

                await Context.Message
                             .DeleteAsync()
                             .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Block channel
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("unblock")]
        [RequireAdministratorPermissions]
        public async Task UnblockChannel()
        {
            if (Context.Channel is IGuildChannel guildChannel)
            {
                BlockedChannelService.RemoveChannel(guildChannel);

                await Context.Message
                             .DeleteAsync()
                             .ConfigureAwait(false);
            }
        }

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
        public async Task InstallSlashCommands()
        {
            IEnumerable<ApplicationCommandProperties> commands = null;

            var buildContext = new SlashCommandBuildContext
                               {
                                   Guild = Context.Guild,
                                   ServiceProvider = Context.ServiceProvider,
                                   CultureInfo = LocalizationGroup.CultureInfo
                               };

            foreach (var type in Assembly.Load("Scruffy.Commands")
                                         .GetTypes()
                                         .Where(obj => typeof(SlashCommandModuleBase).IsAssignableFrom(obj)
                                                    && obj.IsAbstract == false))
            {
                var commandModule = (SlashCommandModuleBase)Activator.CreateInstance(type);
                if (commandModule != null)
                {
                    commands = commands == null
                                   ? commandModule.GetCommands(buildContext)
                                   : commands.Concat(commandModule.GetCommands(buildContext));
                }
            }

            if (commands != null)
            {
                await Context.Guild
                             .BulkOverwriteApplicationCommandsAsync(commands.ToArray())
                             .ConfigureAwait(false);
            }

            await Context.Message
                         .AddReactionAsync(DiscordEmoteService.GetCheckEmote(Context.Client))
                         .ConfigureAwait(false);
        }

        /// <summary>
        /// Uninstall SlashCommands
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("uninstall")]
        [RequireAdministratorPermissions]
        public async Task UninstallSlashCommands()
        {
            await Context.Guild
                         .BulkOverwriteApplicationCommandsAsync(Array.Empty<ApplicationCommandProperties>())
                         .ConfigureAwait(false);

            await Context.Message
                         .AddReactionAsync(DiscordEmoteService.GetCheckEmote(Context.Client))
                         .ConfigureAwait(false);
        }

        /// <summary>
        /// Set SlashCommand permissions
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="role">Role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("set-permissions")]
        [RequireAdministratorPermissions]
        public async Task SetSlashCommandsPermissions(string groupName, IRole role)
        {
            var module = InteractionService.Modules
                                           .FirstOrDefault(obj => obj.SlashGroupName == groupName);

            if (module != null)
            {
                await InteractionService.ModifySlashCommandPermissionsAsync(module, Context.Guild, new ApplicationCommandPermission(role, true))
                                        .ConfigureAwait(false);

                await Context.Message
                             .AddReactionAsync(DiscordEmoteService.GetCheckEmote(Context.Client))
                             .ConfigureAwait(false);
            }
            else
            {
                await Context.Message
                             .AddReactionAsync(DiscordEmoteService.GetCrossEmote(Context.Client))
                             .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Remove SlashCommands permissions
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="role">Role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("remove-permissions")]
        [RequireAdministratorPermissions]
        public async Task RemoveSlashCommandsPermissions(string groupName, IRole role)
        {
            var module = InteractionService.Modules
                                           .FirstOrDefault(obj => obj.SlashGroupName == groupName);

            if (module != null)
            {
                await InteractionService.ModifySlashCommandPermissionsAsync(module, Context.Guild, new ApplicationCommandPermission(role, true))
                                        .ConfigureAwait(false);

                await Context.Message
                             .AddReactionAsync(DiscordEmoteService.GetCheckEmote(Context.Client))
                             .ConfigureAwait(false);
            }
            else
            {
                await Context.Message
                             .AddReactionAsync(DiscordEmoteService.GetCrossEmote(Context.Client))
                             .ConfigureAwait(false);
            }
        }

        #endregion // Methods
    }

    #endregion // SlashCommands

}
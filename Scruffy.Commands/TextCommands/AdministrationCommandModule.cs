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
[HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
public class AdministrationCommandModule : LocatedTextCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Configuration service
    /// </summary>
    public AdministrationService AdministrationService { get; set; }

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
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public async Task Rename(IGuildUser member, [Remainder] string name)
    {
        try
        {
            await AdministrationService.RenameMember(member, name)
                                       .ConfigureAwait(false);
        }
        catch
        {
            await Context.Message
                         .ReplyAsync(LocalizationGroup.GetText("NotAllowedToPerform", "I'm not allowed to perform this action."))
                         .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Rename user
    /// </summary>
    /// <param name="role">Member</param>
    /// <param name="name">name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("rename")]
    [RequireAdministratorPermissions]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public async Task Rename(IRole role, [Remainder] string name)
    {
        try
        {
            await AdministrationService.RenameRole(role, name)
                                       .ConfigureAwait(false);
        }
        catch
        {
            await Context.Message
                         .ReplyAsync(LocalizationGroup.GetText("NotAllowedToPerform", "I'm not allowed to perform this action."))
                         .ConfigureAwait(false);
        }
    }

    #endregion // Methods

    #region Channel

    /// <summary>
    /// Channel commands
    /// </summary>
    [Group("channel")]
    [Alias("c")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
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
}
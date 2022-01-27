
using Scruffy.Services.Administration;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Commands;

/// <summary>
/// Admin commands
/// </summary>
[Group("admin")]
[Aliases("ad")]
[RequireGuild]
[RequireAdministratorPermissions]
[HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
[ModuleLifespan(ModuleLifespan.Transient)]
public class AdministrationCommandModule : LocatedCommandModuleBase
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
    /// <param name="commandContext">Current command context</param>
    /// <param name="member">Member</param>
    /// <param name="name">name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("rename")]
    [RequireAdministratorPermissions]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public Task Rename(CommandContext commandContext, DiscordMember member, [RemainingText] string name)
    {
        return InvokeAsync(commandContext,
                           async commandContextContainer =>
                           {
                               try
                               {
                                   await AdministrationService.RenameMember(member, name)
                                                              .ConfigureAwait(false);
                               }
                               catch (UnauthorizedException)
                               {
                                   await commandContext.RespondAsync(LocalizationGroup.GetText("NotAllowedToPerform", "I'm not allowed to perform this action."))
                                                       .ConfigureAwait(false);
                               }
                           });
    }

    /// <summary>
    /// Rename user
    /// </summary>
    /// <param name="commandContext">Current command context</param>
    /// <param name="role">Member</param>
    /// <param name="name">name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("rename")]
    [RequireAdministratorPermissions]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public Task Rename(CommandContext commandContext, DiscordRole role, [RemainingText] string name)
    {
        return InvokeAsync(commandContext,
                           async commandContextContainer =>
                           {
                               try
                               {
                                   await AdministrationService.RenameRole(role, name)
                                                              .ConfigureAwait(false);
                               }
                               catch (UnauthorizedException)
                               {
                                   await commandContext.RespondAsync(LocalizationGroup.GetText("NotAllowedToPerform", "I'm not allowed to perform this action."))
                                                       .ConfigureAwait(false);
                               }
                           });
    }

    #endregion // Methods

    #region Channel

    /// <summary>
    /// Channel commands
    /// </summary>
    [Group("channel")]
    [Aliases("c")]
    [RequireGuild]
    [RequireAdministratorPermissions]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class AdministrationChannelCommandModule : LocatedCommandModuleBase
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
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("block")]
        [RequireAdministratorPermissions]
        public Task BlockChannel(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   BlockedChannelService.AddChannel(commandContextContainer.Channel);

                                   await commandContext.Message.DeleteAsync()
                                                       .ConfigureAwait(false);
                               });
        }

        /// <summary>
        /// Block channel
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("unblock")]
        [RequireAdministratorPermissions]
        public Task UnblockChannel(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   BlockedChannelService.RemoveChannel(commandContextContainer.Channel);

                                   await commandContext.Message.DeleteAsync()
                                                       .ConfigureAwait(false);
                               });
        }

        #endregion // Methods
    }

    #endregion // Channel
}
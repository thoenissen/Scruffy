using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Scruffy.Services.Administration;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Commands;

/// <summary>
/// Calendar commands
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
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
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
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
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
}
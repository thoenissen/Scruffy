using Discord;
using Discord.Interactions;

using Scruffy.Services.Administration;
using Scruffy.Services.Discord;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Server administration commands
/// </summary>
[Group("admin", "Server administration commands")]
[DefaultPermission(false)]
public class AdministrationSlashCommandModule : SlashCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public AdministrationCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Renaming a member
    /// </summary>
    /// <param name="member">Member</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("rename-member", "Renaming a user")]
    public Task RenameMember([Summary("Member", "Member")]IGuildUser member,
                             [Summary("Name", "New nickname")]string name) => CommandHandler.RenameMember(Context, member, name);

    /// <summary>
    /// Renaming a role
    /// </summary>
    /// <param name="member">Member</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("rename-role", "Renaming a user")]
    public Task RenameRole([Summary("Role", "Role")]IRole member,
                             [Summary("Name", "New name")]string name) => CommandHandler.RenameRole(Context, member, name);

    #endregion // Methods
}
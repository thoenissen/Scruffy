using Discord.Commands;

using Scruffy.Services.Account;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Calendar commands
/// </summary>
[Group("account")]
[Alias("ac")]
[BlockedChannelCheck]
public class AccountCommandModule : LocatedTextCommandModuleBase
{
    #region Command methods

    /// <summary>
    /// Adding an account
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command("add")]
    public Task Add() => ShowMigrationMessage("account");

    /// <summary>
    /// Editing an account
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command("edit")]
    public Task Edit() => ShowMigrationMessage("account");

    /// <summary>
    /// Remove an account
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command("remove")]
    public Task Remove() => ShowMigrationMessage("account");

    /// <summary>
    /// Validation of all accounts
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command("validation")]
    [RequireAdministratorPermissions]
    public Task Validation() => ShowMigrationMessage("guild-admin check");

    #endregion // Command methods

}
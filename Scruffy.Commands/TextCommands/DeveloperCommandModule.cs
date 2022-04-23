using Discord.Commands;

using Scruffy.Services.Developer;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Admin commands
/// </summary>
[Group("developer")]
[Alias("dev", "de")]
[RequireContext(ContextType.Guild)]
[RequireAdministratorPermissions]
public class DeveloperCommandModule : LocatedTextCommandModuleBase
{
    #region Methods

    /// <summary>
    /// Set account name
    /// </summary>
    /// <param name="accountName">name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setAccount")]
    public Task SetAccount(string accountName) => ShowMigrationMessage("account");

    #endregion // Methods
}
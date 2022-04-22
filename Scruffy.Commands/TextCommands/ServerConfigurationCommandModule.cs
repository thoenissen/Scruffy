using Discord;
using Discord.Commands;

using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Configuration the server
/// </summary>
[Group("config")]
[Alias("co")]
[RequireAdministratorPermissions]
public class ServerConfigurationCommandModule : LocatedTextCommandModuleBase
{
    #region Methods

    /// <summary>
    /// Set the server administration role
    /// </summary>
    /// <param name="role">Roles</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("adminRole")]
    [RequireContext(ContextType.Guild)]
    public Task SetAdministrationRole(IRole role) => ShowMigrationMessage("configuration");

    #endregion // Methods
}
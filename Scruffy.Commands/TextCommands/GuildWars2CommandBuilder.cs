using Discord.Commands;

using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Guild Wars 2 commands
/// </summary>
[Group("gw2")]
[BlockedChannelCheck]
public class GuildWars2CommandBuilder : LocatedTextCommandModuleBase
{
    #region Command methods

    /// <summary>
    /// Creation of a one time reminder
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("quaggan")]
    public Task Quaggan() => ShowMigrationMessage("gw2 quaggan");

    /// <summary>
    /// Next update
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("update")]
    public Task Update() => ShowMigrationMessage("gw2 update");

    #endregion // Command methods
}
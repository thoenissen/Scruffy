using Discord.Commands;

using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Calendar commands
/// </summary>
[Group("information")]
[Alias("info", "i")]
[BlockedChannelCheck]
public class InformationCommandModule : LocatedTextCommandModuleBase
{
    #region Methods

    /// <summary>
    /// Show information about Scruffy
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command]
    public Task Info() => ShowMigrationMessage("info");

    #endregion // Methods
}
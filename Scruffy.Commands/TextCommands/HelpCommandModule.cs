using Discord.Commands;

using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Calendar commands
/// </summary>
[Group("help")]
[Alias("h")]
[BlockedChannelCheck]
public class HelpCommandModule : LocatedTextCommandModuleBase
{
    #region Methods

    /// <summary>
    /// Adding a one time event
    /// </summary>
    /// <param name="command">Command</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command]
    public Task Info([Remainder]string command = null) => ShowMigrationMessage("help");

    #endregion // Methods
}
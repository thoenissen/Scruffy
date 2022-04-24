using Discord.Commands;

using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// GIF commands
/// </summary>
[Group("gif")]
[Alias("gi")]
[BlockedChannelCheck]
public class GifCommandModule : LocatedTextCommandModuleBase
{
    #region Methods

    /// <summary>
    /// GIF related to a string
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command]
    public Task GroupCommand([Remainder] string searchTerm) => ShowMigrationMessage("gif");

    #endregion // Methods
}
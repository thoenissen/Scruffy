using Discord.Commands;

using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Searching the web
/// </summary>
[BlockedChannelCheck]
[Group("search")]
[Alias("se")]
public class SearchCommandModule : LocatedTextCommandModuleBase
{
    #region Command methods

    /// <summary>
    /// Searching google
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command("google")]
    public Task Google([Remainder] string searchTerm) => ShowMigrationMessage("search google");

    /// <summary>
    /// Searching google
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command("gw2wiki")]
    public Task GW2Wiki([Remainder] string searchTerm) => ShowMigrationMessage("search gw2wiki");

    #endregion // Command methods
}
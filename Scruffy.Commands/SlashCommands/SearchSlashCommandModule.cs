using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.Search;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Searching the web
/// </summary>
[Group("search", "Search commands")]
public class SearchSlashCommandModule : SlashCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public SearchCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Searching for result on Google
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("google", "Searching for result on Google")]
    public Task Google([Summary("Search-term", "The search term which should be used")] string searchTerm) => CommandHandler.Google(Context, searchTerm);

    /// <summary>
    /// Searching for result on the official Guild Wars 2 Wiki
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("gw2wiki", "Searching for result on the official Guild Wars 2 Wiki")]
    public Task GW2Wiki([Summary("Search-term", "The search term which should be used")] string searchTerm) => CommandHandler.GW2Wiki(Context, searchTerm);

    #endregion // Methods
}
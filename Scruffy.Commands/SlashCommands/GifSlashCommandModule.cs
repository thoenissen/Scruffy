using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.Gifs;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Gif commands
/// </summary>
[Group("gif", "Gif related commands")]
public class GifSlashCommandModule : SlashCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public GifCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Command methods

    /// <summary>
    /// Creation of a one time reminder
    /// </summary>
    /// <param name="searchTerm">gif search term</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("random", "Posting random GIFs")]
    public Task Gif(string searchTerm) => CommandHandler.Gif(Context, searchTerm);

    #endregion // Command methods
}
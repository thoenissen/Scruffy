using Discord.Interactions;

using Scruffy.Services.Discord;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Help slash commands
/// </summary>
public class HelpSlashCommandModule : SlashCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Visualizer
    /// </summary>
    public CommandHelpService CommandHelpService { get; set; }

    #endregion // Properties

    #region Commands

    /// <summary>
    /// Display command help
    /// </summary>
    /// <param name="command">Command</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [SlashCommand("help", "Shows the Scruffy help message", true)]
    public async Task Show([Summary("command", "Name of the command")]string command = null)
    {
        await CommandHelpService.ShowHelp(Context, command)
                                .ConfigureAwait(false);
    }

    #endregion // Commands
}
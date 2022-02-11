using Discord.Interactions;

using Scruffy.Services.Discord;

namespace Scruffy.Commands.SlashCommands
{
    /// <summary>
    /// Help slash commands
    /// </summary>
    [Group("help", "Command to display the command help.")]
    public class HelpSlashCommandModule : LocatedInteractionModuleBase
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
        [SlashCommand("help", "Display the command help.", true)]
        public async Task Show([Summary("command", "Name of the command.")]string command = null)
        {
            await CommandHelpService.ShowHelp(Context, command)
                                    .ConfigureAwait(false);
        }

        #endregion // Commands
    }
}

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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [DefaultMemberPermissions(Discord.GuildPermission.SendMessages)]
    [SlashCommand("help", "Shows the Scruffy help message", true)]
    public async Task Show()
    {
        await CommandHelpService.ShowHelp(Context)
                                .ConfigureAwait(false);
    }

    #endregion // Commands
}
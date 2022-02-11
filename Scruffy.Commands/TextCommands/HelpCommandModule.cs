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
    #region Properties

    /// <summary>
    /// Visualizer
    /// </summary>
    public CommandHelpService CommandHelpService { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Adding a one time event
    /// </summary>
    /// <param name="command">Command</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public async Task Info([Remainder]string command = null)
    {
        await CommandHelpService.ShowHelp(Context, command)
                                .ConfigureAwait(false);
    }

    #endregion // Methods
}
using Discord.Interactions;

using Scruffy.Data.Enumerations.DpsReport;
using Scruffy.Services.Discord;
using Scruffy.Services.GuildWars2;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Log slash commands
/// </summary>
[Group("logs", "Log commands")]
public class LogSlashCommandModule : SlashCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Log command handler
    /// </summary>
    public LogCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Commands

    /// <summary>
    /// Full list of logs of certain day
    /// </summary>
    /// <param name="type">Type of DPS-reports</param>
    /// <param name="day">Day</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("full", "Shows all fractal/strike/raid logs of a given day")]
    public async Task Logs([Summary("type", "Which type of logs should be used?")] DpsReportType type = DpsReportType.All, [Summary("day", "Day of the logs (yyyy-MM-dd)")] string day = null)
    {
        await Context.DeferAsync()
                     .ConfigureAwait(false);

        await CommandHandler.Logs(Context, type, day)
                            .ConfigureAwait(false);
    }

    #endregion // Commands
}
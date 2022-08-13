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
    /// All logs of a certain day
    /// </summary>
    /// <param name="type">Type of DPS-reports</param>
    /// <param name="day">Day</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("all", "Shows all fractal/strike/raid logs of a given day")]
    public async Task FullLogs([Summary("type", "Which type of logs should be used?")] DpsReportType type = DpsReportType.All, [Summary("day", "Day of the logs (yyyy-MM-dd)")] string day = null)
    {
        await Context.DeferAsync()
                     .ConfigureAwait(false);

        await CommandHandler.Logs(Context, type, day, false)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Summary of logs of a certain day
    /// </summary>
    /// <param name="type">Type of DPS-reports</param>
    /// <param name="day">Day</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("summary", "Shows the best fractal/strike/raid logs of a given day")]
    public async Task SummarizeLogs([Summary("type", "Which type of logs should be used?")] DpsReportType type = DpsReportType.All, [Summary("day", "Day of the logs (yyyy-MM-dd)")] string day = null)
    {
        await Context.DeferAsync()
                     .ConfigureAwait(false);

        await CommandHandler.Logs(Context, type, day, true)
                            .ConfigureAwait(false);
    }

    #endregion // Commands
}
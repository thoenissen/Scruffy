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
    public async Task FullLogs([Summary("type", "Which type of logs should be used?")] DpsReportType type = DpsReportType.All, [Summary("day", "Day of the logs (dd.MM)")] string day = null)
    {
        await Context.DeferAsync()
                     .ConfigureAwait(false);

        await CommandHandler.PostLogs(Context, type, day, false)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Summary of logs of a certain day
    /// </summary>
    /// <param name="type">Type of DPS-reports</param>
    /// <param name="day">Day</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("summary", "Shows the best fractal/strike/raid logs of a given day")]
    public async Task SummarizeLogs([Summary("type", "Which type of logs should be used?")] DpsReportType type = DpsReportType.All, [Summary("day", "Day of the logs (dd.MM)")] string day = null)
    {
        await Context.DeferAsync()
                     .ConfigureAwait(false);

        await CommandHandler.PostLogs(Context, type, day, true)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// History of last <paramref name="count"/> of <paramref name="type"/> tries
    /// </summary>
    /// <param name="type">Type to get the logs for</param>
    /// <param name="count">Max count logs to search</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("history", "Shows the last (count) successful tries of (type)")]
    public async Task LogHistory(DpsReportGroup type, int count = 5)
    {
        await Context.DeferAsync()
                     .ConfigureAwait(false);

        await CommandHandler.PostHistory(Context, type, count)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// History of last <paramref name="count"/> of <paramref name="type"/> tries
    /// </summary>
    /// <param name="type">Type to get the logs for</param>
    /// <param name="count">Max count logs to search</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("chart", "Post a chart showing the last (count) of successful tries of (type)")]
    public async Task LogChart(DpsReportGroup type, int count = 10)
    {
        await Context.DeferAsync()
                     .ConfigureAwait(false);

        await CommandHandler.PostChart(Context, type, count)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Exporting urls of all logs since the given date
    /// </summary>
    /// <param name="since">Since day</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("export", "Export of log urls")]
    public Task Export([Summary("since-day", "Logs since the given day. (dd.MM.yyyy)")] string since) => CommandHandler.ExportLogs(Context, since);

    #endregion // Commands
}
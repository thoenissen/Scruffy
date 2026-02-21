using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.GuildWars2.DpsReports;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Log slash commands
/// </summary>
[Group("logs-beta", "Log commands")]
public class LogsBetaSlashCommandModule : SlashCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Log command handler
    /// </summary>
    public LogBetaCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Commands

    /// <summary>
    /// Imports logs from dps.report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("import", "Imports logs from dps.report")]
    public async Task Import()
    {
        await CommandHandler.Import(Context)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Display logs of current day
    /// </summary>
    /// <param name="dateString">Date string</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("day", "Display logs of a day")]
    public async Task Day([Summary("date", "Date (dd.mm; dd.mm.yyyy)")]string dateString = null)
    {
        await CommandHandler.Day(Context, dateString)
                            .ConfigureAwait(false);
    }

    #endregion // Commands
}
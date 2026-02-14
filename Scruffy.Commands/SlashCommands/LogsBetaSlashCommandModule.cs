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

    #endregion // Commands
}
using Discord;
using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.GuildWars2;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Admin log slash commands
/// </summary>
[Group("logs-admin", "Log admin commands")]
public class LogsAdminSlashCommandModule : SlashCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Log command handler
    /// </summary>
    public LogCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Commands

    /// <summary>
    /// Check logs of a given user
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="count">Count</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("check", "Check logs")]
    public async Task Check([Summary("user", "User")]IUser user, [Summary("count", "Count of logs to check")]int? count = 0)
    {
        await Context.DeferProcessing()
                     .ConfigureAwait(false);

        await CommandHandler.CheckLogs(Context, user, count ?? 50)
                            .ConfigureAwait(false);
    }

    #endregion // Commands
}
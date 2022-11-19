using Discord;
using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.Guild;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Guild commands
/// </summary>
[Group("guild", "Guild related commands")]
public class GuildSlashCommandHandler : SlashCommandModuleBase
{
    #region Enumeration

    /// <summary>
    /// Typ of unlocks
    /// </summary>
    public enum GuildBankUnlocks
    {
        Dyes
    }

    #endregion // Enumeration

    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public GuildCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Execution of some guild bank checks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("bank-check", "Execution of some guild bank checks")]
    public Task BankCheck() => CommandHandler.GuildBankCheck(Context);

    /// <summary>
    /// Check if everything of the given type stored in the guild bank is unlocked
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("bank-unlocks", "Check if items of the given type are unlocked")]
    public async Task BankUnlocks([Summary("Type", "Item type to be checked")]GuildBankUnlocks type)
    {
        switch (type)
        {
            case GuildBankUnlocks.Dyes:
                {
                    await CommandHandler.GuildBankUnlocksDyes(Context)
                                        .ConfigureAwait(false);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    /// <summary>
    /// Personal ranking data
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("ranking-me", "Personal guild ranking overview")]
    public async Task PostPersonalRankingOverview()
    {
        await CommandHandler.PostPersonalRankingOverview(Context, Context.Member)
                            .ConfigureAwait(false);

        await CommandHandler.PostPersonalRankingHistoryTypeOverview(Context, Context.Member)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Personal ranking data compare
    /// </summary>
    /// <param name="compareUser">Compare user</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("ranking-compare", "Personal guild ranking compare")]
    public async Task PostPersonalCompareOverview([Summary("User", "Compare your points with the given user")]IGuildUser compareUser)
    {
        await CommandHandler.PostPersonalCompareOverview(Context, Context.Member, compareUser)
                            .ConfigureAwait(false);
    }

    #endregion // Methods
}
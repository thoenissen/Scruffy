using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.Guild;

namespace Scruffy.Commands.SlashCommands
{
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
        public GuildCommandHandler GuildCommandHandler { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Execution of some guild bank checks
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [SlashCommand("bank-check", "Execution of some guild bank checks")]
        public Task BankCheck() => GuildCommandHandler.GuildBankCheck(Context);

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
                        await GuildCommandHandler.GuildBankUnlocksDyes(Context)
                                                 .ConfigureAwait(false);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// Posting of random guild emblems
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [SlashCommand("random-emblem", "Posting of random guild emblems")]
        public Task RandomEmblems([Summary("Count", "Count of emblems")]
                                  [MinValue(1)]
                                  [MaxValue(10)]
                                  int count)
            => GuildCommandHandler.RandomEmblems(Context, count);

        /// <summary>
        /// Personal ranking data
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [SlashCommand("ranking-me", "Personal guild ranking overview")]
        public Task PostPersonalRankingOverview() => GuildCommandHandler.PostPersonalRankingOverview(Context, Context.Member);

        #endregion // Methods
    }
}
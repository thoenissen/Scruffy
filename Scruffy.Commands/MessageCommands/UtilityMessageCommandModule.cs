using Discord;
using Discord.Interactions;

using Scruffy.Services.Core;
using Scruffy.Services.Discord;

namespace Scruffy.Commands.MessageCommands
{
    /// <summary>
    /// Utility commands
    /// </summary>
    public class UtilityMessageCommandModule : MessageCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Command handler
        /// </summary>
        public UtilityCommandHandler CommandHandler { get; set; }

        #endregion // Properties

        #region Commands

        /// <summary>
        /// Set reactions
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [MessageCommand("Add reactions")]
        public async Task AddReactions(IUserMessage message)
        {
            var processingMessage = await Context.DeferProcessing()
                                                 .ConfigureAwait(false);

            await CommandHandler.AddReactions(message)
                                .ConfigureAwait(false);

            await processingMessage.DeleteAsync()
                                   .ConfigureAwait(false);
        }

        /// <summary>
        /// Set reactions
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [MessageCommand("Remove reactions")]
        public async Task RemoveReactions(IUserMessage message)
        {
            var processingMessage = await Context.DeferProcessing()
                                                 .ConfigureAwait(false);

            await CommandHandler.RemoveReactions(message)
                                .ConfigureAwait(false);

            await processingMessage.DeleteAsync()
                                   .ConfigureAwait(false);
        }

        /// <summary>
        /// Repost message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [MessageCommand("Repost message")]
        public async Task RepostMessage(IUserMessage message)
        {
            var processingMessage = await Context.DeferProcessing()
                                                 .ConfigureAwait(false);

            await CommandHandler.RepostMessage(message)
                                .ConfigureAwait(false);

            await processingMessage.DeleteAsync()
                                   .ConfigureAwait(false);
        }

        #endregion // Commands
    }
}
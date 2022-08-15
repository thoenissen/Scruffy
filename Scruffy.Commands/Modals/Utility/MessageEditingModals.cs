using Discord.Interactions;

using Scruffy.Services.Core;
using Scruffy.Services.Core.Modals;
using Scruffy.Services.Discord;

namespace Scruffy.Commands.Modals.Utility
{
    /// <summary>
    /// Message editing modals
    /// </summary>
    public class MessageEditingModals : LocatedInteractionModuleBase
    {
        #region Properties

        /// <summary>
        /// Command handler
        /// </summary>
        public UtilityCommandHandler CommandHandler { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Add link
        /// </summary>
        /// <param name="channelId">Channel id</param>
        /// <param name="messageId">Message id</param>
        /// <param name="modal">Modal input</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [ModalInteraction($"{AddLinkModalData.CustomIdPrefix};*;*")]
        public async Task AddLink(ulong channelId, ulong messageId, AddLinkModalData modal)
        {
            await Context.DeferAsync()
                         .ConfigureAwait(false);

            await CommandHandler.AddLink(Context, channelId, messageId, modal.Name, modal.Link)
                                .ConfigureAwait(false);
        }

        #endregion // Methods
    }
}
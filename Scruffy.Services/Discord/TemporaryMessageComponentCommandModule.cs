using Discord.Interactions;
using Discord.WebSocket;

namespace Scruffy.Services.Discord
{
    /// <summary>
    /// General module for processing temporary message components
    /// </summary>
    public class TemporaryMessageComponentCommandModule : LocatedInteractionModuleBase
    {
        #region Methods

        /// <summary>
        /// Temporary button
        /// </summary>
        /// <param name="identification">Identification</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [ComponentInteraction("temporary;button;*")]
        public async Task ExecuteTemporaryButton(string identification)
        {
            if (Context.Interaction is SocketMessageComponent component)
            {
                Context.Interactivity.CheckButtonComponent(identification, component);
            }
            else
            {
                await Context.Interaction
                             .DeferAsync()
                             .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Temporary select menu
        /// </summary>
        /// <param name="identification">Identification</param>
        /// <param name="unused">Unused</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [ComponentInteraction("temporary;selectMenu;*")]
        public async Task ExecuteTemporarySelectMenu(string identification, string[] unused)
        {
            if (Context.Interaction is SocketMessageComponent component)
            {
                Context.Interactivity.CheckSelectMenuComponent(identification, component);
            }
            else
            {
                await Context.Interaction
                             .DeferAsync()
                             .ConfigureAwait(false);
            }
        }

        #endregion // Methods
    }
}

using Discord;
using Discord.WebSocket;

using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Core
{
    /// <summary>
    /// Utility commands
    /// </summary>
    public class UtilityCommandHandler : LocatedServiceBase
    {
        #region Fields

        /// <summary>
        /// Discord client
        /// </summary>
        private readonly DiscordSocketClient _discordClient;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="discordClient">Discord client</param>
        public UtilityCommandHandler(LocalizationService localizationService, DiscordSocketClient discordClient)
            : base(localizationService)
        {
            _discordClient = discordClient;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Set the same reactions as of the given message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddReactions(IMessage message)
        {
            if (message.Channel is ITextChannel textChannel)
            {
                if (await textChannel.GetMessageAsync(message.Id).ConfigureAwait(false) is IUserMessage userMessage)
                {
                    foreach (var reaction in userMessage.Reactions)
                    {
                        await message.AddReactionAsync(reaction.Key)
                                     .ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Remove the same reactions as of the given message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RemoveReactions(IMessage message)
        {
            if (message.Channel is ITextChannel textChannel)
            {
                if (await textChannel.GetMessageAsync(message.Id).ConfigureAwait(false) is IUserMessage userMessage)
                {
                    foreach (var reaction in userMessage.Reactions)
                    {
                        await message.RemoveReactionAsync(reaction.Key, _discordClient.CurrentUser)
                                     .ConfigureAwait(false);
                    }
                }
            }
        }

        #endregion // Methods
    }
}
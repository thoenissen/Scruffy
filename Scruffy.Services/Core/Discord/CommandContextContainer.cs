using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

using Scruffy.Services.Core.Discord.Interfaces;

namespace Scruffy.Services.Core.Discord
{
    /// <summary>
    /// CommandContext wrapper
    /// </summary>
    public class CommandContextContainer : ICommandContext
    {
        #region Static methods

        /// <summary>
        /// Create a wrapper from a normal CommandContext
        /// </summary>
        /// <param name="commandContext">CommandContext</param>
        /// <returns>ICommandContext-implementation</returns>
        public static ICommandContext FromCommandContext(CommandContext commandContext)
        {
            return new CommandContextContainer
                   {
                       Client = commandContext.Client,
                       Guild = commandContext.Guild,
                       Message = commandContext.Message,
                       Channel = commandContext.Channel,
                       User = commandContext.User
                   };
        }

        /// <summary>
        /// Switching to a direct message context
        /// </summary>
        /// <param name="commandContext">CommandContext</param>
        /// <returns>ICommandContext-implementation</returns>
        public static async Task<ICommandContext> SwitchToDirectMessageContext(CommandContext commandContext)
        {
            return new CommandContextContainer
                   {
                       Client = commandContext.Client,
                       Message = commandContext.Message,
                       Channel = commandContext.Channel.IsPrivate
                                     ? commandContext.Channel
                                     : await commandContext.Member
                                                           .CreateDmChannelAsync()
                                                           .ConfigureAwait(false),
                       User = commandContext.User
                   };
        }

        #endregion // Static methods

        #region ICommandContext

        /// <summary>
        /// Current discord client
        /// </summary>
        public DiscordClient Client { get; private set; }

        /// <summary>
        /// Current guild
        /// </summary>
        public DiscordGuild Guild { get; private set; }

        /// <summary>
        /// User message
        /// </summary>
        public DiscordMessage Message { get; private set; }

        /// <summary>
        /// Current channel
        /// </summary>
        public DiscordChannel Channel { get; private set; }

        /// <summary>
        /// Current user
        /// </summary>
        public DiscordUser User { get; private set; }

        #endregion // ICommandContext
    }
}
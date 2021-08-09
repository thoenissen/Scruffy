using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace Scruffy.Services.Core.Discord
{
    /// <summary>
    /// CommandContext wrapper
    /// </summary>
    public class CommandContextContainer
    {
        #region Constructor

        /// <summary>
        /// Create a wrapper from a normal CommandContext
        /// </summary>
        /// <param name="commandContext">CommandContext</param>
        /// <returns>ICommandContext-implementation</returns>
        public CommandContextContainer(CommandContext commandContext)
        {
            Client = commandContext.Client;
            Guild = commandContext.Guild;
            LastUserMessage = Message = commandContext.Message;
            Channel = commandContext.Channel;
            User = commandContext.User;
            Member = commandContext.Member;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Switching to a direct message context
        /// </summary>
        /// <returns>ICommandContext-implementation</returns>
        public async Task SwitchToDirectMessageContext()
        {
            if (Channel.IsPrivate == false)
            {
                Channel = await Member.CreateDmChannelAsync()
                                      .ConfigureAwait(false);

                Member = null;
            }
        }

        #endregion // Methods

        #region Properties

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

        /// <summary>
        /// Current member
        /// </summary>
        public DiscordMember Member { get; private set; }

        /// <summary>
        /// Last user message
        /// </summary>
        public DiscordMessage LastUserMessage { get; internal set; }

        #endregion // Properties
    }
}
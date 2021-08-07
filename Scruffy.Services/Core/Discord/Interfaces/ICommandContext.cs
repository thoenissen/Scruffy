using DSharpPlus;
using DSharpPlus.Entities;

namespace Scruffy.Services.Core.Discord.Interfaces
{
    /// <summary>
    /// Current command context
    /// </summary>
    public interface ICommandContext
    {
        /// <summary>
        /// Current discord client
        /// </summary>
        DiscordClient Client { get; }

        /// <summary>
        /// Current guild
        /// </summary>
        DiscordGuild Guild { get; }

        /// <summary>
        /// User message which started the interaction
        /// </summary>
        DiscordMessage Message { get; }

        /// <summary>
        /// Current channel
        /// </summary>
        DiscordChannel Channel { get; }

        /// <summary>
        /// user
        /// </summary>
        DiscordUser User { get; }
    }
}

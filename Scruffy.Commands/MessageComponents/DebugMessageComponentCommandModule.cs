using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands.MessageComponents
{
    /// <summary>
    /// Debug message component commands
    /// </summary>
    [MessageComponentCommandGroup(Group)]
    public class DebugMessageComponentCommandModule : MessageComponentCommandModule
    {
        #region Constants

        /// <summary>
        /// Group
        /// </summary>
        public const string Group = "debug";

        /// <summary>
        /// Command ping
        /// </summary>
        public const string CommandPing = "ping";

        #endregion // Constants

        #region Commands

        /// <summary>
        /// Ping
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [MessageComponentCommand(CommandPing)]
        public async Task Ping()
        {
            await Component.DeferAsync()
                           .ConfigureAwait(false);

            await Component.Channel
                           .SendMessageAsync("Pong!")
                           .ConfigureAwait(false);
        }

        #endregion // Commands
    }
}

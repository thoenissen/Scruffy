using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.Extensions.DependencyInjection;

namespace Scruffy.Services.Core.Discord.Attributes
{
    /// <summary>
    /// Check if channel is blocked for commands
    /// </summary>
    public class BlockedChannelCheckAttribute : CheckBaseAttribute
    {
        #region Fields

        /// <summary>
        /// Service
        /// </summary>
        private static BlockedChannelService _service;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        static BlockedChannelCheckAttribute()
        {
            _service = GlobalServiceProvider.Current.GetServiceProvider().GetService<BlockedChannelService>();
        }

        #endregion // Constructor

        #region CheckBaseAttribute

        /// <summary>
        /// Asynchronously checks whether this command can be executed within given context.
        /// </summary>
        /// <param name="ctx">Context to check execution ability for.</param>
        /// <param name="help">Whether this check is being executed from help or not. This can be used to probe whether command can be run without setting off certain fail conditions (such as cooldowns).</param>
        /// <returns>Whether the command can be executed in given context.</returns>
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) => Task.FromResult(_service.IsChannelBlocked(ctx, ctx.Channel) == false);

        #endregion // CheckBaseAttribute
    }
}

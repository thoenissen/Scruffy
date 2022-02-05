using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core;

namespace Scruffy.Services.Discord.Attributes;

/// <summary>
/// Check if channel is blocked for commands
/// </summary>
public class BlockedChannelCheckAttribute : PreconditionAttribute
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

    #region PreconditionAttribute

    /// <summary>
    /// Checks if the <paramref name="command" /> has the sufficient permission to be executed.
    /// </summary>
    /// <param name="context">The context of the command.</param>
    /// <param name="command">The command being executed.</param>
    /// <param name="services">The service collection used for dependency injection.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        return Task.FromResult(_service.IsChannelBlocked(context) == false
                                   ? PreconditionResult.FromSuccess()
                                   : PreconditionResult.FromError("The channel is blocked for commands."));
    }

    #endregion // PreconditionAttribute
}
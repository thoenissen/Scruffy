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
    private static readonly BlockedChannelService Service;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    static BlockedChannelCheckAttribute()
    {
        Service = ServiceProviderContainer.Current.GetServiceProvider().GetService<BlockedChannelService>();
    }

    #endregion // Constructor

    #region PreconditionAttribute

    /// <inheritdoc/>
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        return Task.FromResult(Service.IsChannelBlocked(context) == false
                                   ? PreconditionResult.FromSuccess()
                                   : PreconditionResult.FromError("The channel is blocked for commands."));
    }

    #endregion // PreconditionAttribute
}
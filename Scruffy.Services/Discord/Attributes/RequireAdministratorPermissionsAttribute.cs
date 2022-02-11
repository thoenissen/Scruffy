using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core;
using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.Services.Discord.Attributes;

/// <summary>
/// Administrator permissions
/// </summary>
public class RequireAdministratorPermissionsAttribute : PreconditionAttribute
{
    #region Fields

    /// <summary>
    /// Service
    /// </summary>
    private static AdministrationPermissionsValidationService _service;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    static RequireAdministratorPermissionsAttribute()
    {
        _service = GlobalServiceProvider.Current.GetServiceProvider().GetService<AdministrationPermissionsValidationService>();
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
    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        return await _service.CheckPermissions(context as IContextContainer)
                             .ConfigureAwait(false)
                   ? PreconditionResult.FromSuccess()
                   : PreconditionResult.FromError("Invalid permission.");
    }

    #endregion // PreconditionAttribute
}
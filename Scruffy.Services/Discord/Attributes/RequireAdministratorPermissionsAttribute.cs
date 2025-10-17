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
    private static readonly AdministrationPermissionsValidationService _service;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    static RequireAdministratorPermissionsAttribute()
    {
        _service = ServiceProviderContainer.Current.GetServiceProvider().GetService<AdministrationPermissionsValidationService>();
    }

    #endregion // Constructor

    #region PreconditionAttribute

    /// <inheritdoc/>
    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        return await _service.CheckPermissions(context as IContextContainer)
                             .ConfigureAwait(false)
                   ? PreconditionResult.FromSuccess()
                   : PreconditionResult.FromError("Invalid permission.");
    }

    #endregion // PreconditionAttribute
}
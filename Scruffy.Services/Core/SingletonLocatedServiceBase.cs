using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Core;

/// <summary>
/// Base class for singleton services
/// </summary>
public class SingletonLocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Localization service
    /// </summary>
    private LocalizationService _localizationService;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Localization group
    /// </summary>
    public LocalizationGroup LocalizationGroup => field ??= _localizationService.GetGroup(GetType().Name);

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <remarks>When this method is called all services are registered and can be resolved.  But not all singleton services may be initialized. </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public virtual Task Initialize(IServiceProvider serviceProvider)
    {
        _localizationService = serviceProvider.GetRequiredService<LocalizationService>();

        return Task.CompletedTask;
    }

    #endregion // Methods
}
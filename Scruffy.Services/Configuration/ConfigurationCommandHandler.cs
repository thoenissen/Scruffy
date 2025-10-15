using Scruffy.Services.Configuration.DialogElements;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Configuration;

/// <summary>
/// Configuration commands
/// </summary>
public class ConfigurationCommandHandler : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Permission validation service
    /// </summary>
    private readonly AdministrationPermissionsValidationService _permissionsValidationService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="permissionsValidationService">Permission validation service</param>
    public ConfigurationCommandHandler(LocalizationService localizationService, AdministrationPermissionsValidationService permissionsValidationService)
        : base(localizationService)
    {
        _permissionsValidationService = permissionsValidationService;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Server configuration
    /// </summary>
    /// <param name="context">Context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Configure(InteractionContextContainer context)
    {
        if (await _permissionsValidationService.CheckPermissions(context)
                                               .ConfigureAwait(false))
        {
            var dialogHandler = new DialogHandler(context);

            await using (dialogHandler.ConfigureAwait(false))
            {
                await dialogHandler.Run<ServerConfigurationDialogElement, bool>()
                                          .ConfigureAwait(false);

                await dialogHandler.DeleteMessages()
                                   .ConfigureAwait(false);
            }
        }
        else
        {
            await context.ReplyAsync(LocalizationGroup.GetText("MissingPermissions", "You don't have the required permissions to execute this command."))
                         .ConfigureAwait(false);
        }
    }

    #endregion // Methods
}
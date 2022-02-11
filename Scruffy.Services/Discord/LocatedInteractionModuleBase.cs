using System.Net.Http;

using Discord.Interactions;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;

namespace Scruffy.Services.Discord;

/// <summary>
/// Interaction module base class with localization services
/// </summary>
public class LocatedInteractionModuleBase : InteractionModuleBase<InteractionContextContainer>
{
    #region Fields

    /// <summary>
    /// Localization group
    /// </summary>
    private LocalizationGroup _localizationGroup;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Localization service
    /// </summary>
    public LocalizationService LocalizationService { protected get; set; }

    /// <summary>
    /// User management service
    /// </summary>
    public UserManagementService UserManagementService { protected get; set; }

    /// <summary>
    /// HttpClient-Factory
    /// </summary>
    public IHttpClientFactory HttpClientFactory { protected get; set; }

    /// <summary>
    /// Localization group
    /// </summary>
    public LocalizationGroup LocalizationGroup => _localizationGroup ??= LocalizationService.GetGroup(GetType().Name);

    #endregion // Properties

    #region ModuleBase

    /// <summary>
    /// Before execution
    /// </summary>
    /// <param name="command">Command</param>
    public override void BeforeExecute(ICommandInfo command)
    {
        Context.Command = command;
    }

    #endregion // ModuleBase
}
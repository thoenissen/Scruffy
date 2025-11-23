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
    public LocalizationGroup LocalizationGroup => field ??= LocalizationService.GetGroup(GetType().Name);

    #endregion // Properties

    #region ModuleBase

    /// <inheritdoc/>
    public override void BeforeExecute(ICommandInfo command)
    {
        Context.Command = command;
    }

    #endregion // ModuleBase
}
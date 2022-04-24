using System.Net.Http;

using Discord.Commands;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;

namespace Scruffy.Services.Discord;

/// <summary>
/// Command module base class with localization services
/// </summary>
public class LocatedTextCommandModuleBase : ModuleBase<CommandContextContainer>
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
    protected override void BeforeExecute(CommandInfo command)
    {
        Context.Command = command;
    }

    #endregion // ModuleBase

    #region Methods

    /// <summary>
    /// Post a migration hint
    /// </summary>
    /// <param name="slashCommandName">Name of the new Slash Command</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected async Task ShowMigrationMessage(string slashCommandName)
    {
        await Context.ReplyAsync(LocalizationService.GetGroup(nameof(LocatedTextCommandModuleBase)).GetFormattedText("SlashCommandMigrationHint", "Text commands aren't available anymore. You can execute this command with the new slash command `/{0}`.", slashCommandName))
                     .ConfigureAwait(false);
    }

    #endregion // Methods
}
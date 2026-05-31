using Discord.Interactions;

using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Services.Discord;
using Scruffy.Services.GuildWars2;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Guild Wars 2 beta commands
/// </summary>
[Group("gw2-beta", "Guild Wars 2 related commands")]
public class GuildWars2BetaSlashCommandModule : SlashCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public GuildWars2CommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Command methods

    /// <summary>
    /// Wizard's Vault rewards
    /// </summary>
    /// <param name="mode">Mode</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("vault", "Guild Wars 2 Wizard's Vault rewards")]
    public Task Vault([Summary("Mode", "Select which rewards should be shown")] WizardVaultMode mode = WizardVaultMode.Recommended)
    {
        return CommandHandler.ListWizardVault(Context, mode);
    }

    #endregion // Command methods
}
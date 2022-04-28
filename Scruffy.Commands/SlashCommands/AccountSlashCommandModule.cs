using Discord.Interactions;

using Scruffy.Services.Account;
using Scruffy.Services.Discord;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Account management
/// </summary>
[DontAutoRegister]
public class AccountSlashCommandModule : SlashCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public AccountCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Account configuration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("account", "Account configuration")]
    public Task ConfigureAccount() => CommandHandler.ConfigureAccount(Context);

    #endregion // Methods
}
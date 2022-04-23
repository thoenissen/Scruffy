using Discord.Interactions;

using Scruffy.Services.Account;
using Scruffy.Services.Account.Modals;
using Scruffy.Services.Discord;

namespace Scruffy.Commands.Modals.Account;

/// <summary>
/// Account modals
/// </summary>
public class AccountModals : LocatedInteractionModuleBase
{
    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public AccountCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Add or refresh a Guild Wars 2 account
    /// </summary>
    /// <param name="modal">Modal input</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [ModalInteraction(GuildWarsAccountModal.CustomId)]
    public Task AddOrRefreshGuildWarsAccount(GuildWarsAccountModal modal) => CommandHandler.AddOrRefreshGuildWarsAccount(Context, modal.APIKey);

    /// <summary>
    /// Add or refresh a GW2 DPS Report user token
    /// </summary>
    /// <param name="modal">Modal input</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [ModalInteraction(DpsReportUserTokenModal.CustomId)]
    public Task AddOrRefreshDpsReportUserToken(DpsReportUserTokenModal modal) => CommandHandler.AddOrRefreshDpsReportUserToken(Context, modal.UserToken);

    /// <summary>
    /// Add or refresh a GitHub account
    /// </summary>
    /// <param name="modal">Modal input</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [ModalInteraction(GitHubAccountModal.CustomId)]
    public Task AddOrRefreshGitHubAccount(GitHubAccountModal modal) => CommandHandler.AddOrRefreshGitHubAccount(Context, modal.AccountName);

    /// <summary>
    /// Add or refresh a personal data
    /// </summary>
    /// <param name="modal">Modal input</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [ModalInteraction(PersonalDataModal.CustomId)]
    public Task AddOrRefreshPersonalData(PersonalDataModal modal) => CommandHandler.AddOrRefreshPersonalData(Context, modal.Name, modal.Birthday);

    #endregion // Methods
}
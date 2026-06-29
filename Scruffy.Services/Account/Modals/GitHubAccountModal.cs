using Discord.Interactions;

namespace Scruffy.Services.Account.Modals;

/// <summary>
/// Add or refresh a GitHub Account
/// </summary>
public class GitHubAccountModal : IModal
{
    #region Constants

    /// <summary>
    /// Custom id
    /// </summary>
    public const string CustomId = "modal;acccount;add-or-refresh-github-account";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// API-Key
    /// </summary>
    [InputLabel("Account name")]
    [RequiredInput(false)]
    [ModalTextInput(nameof(AccountName))]
    public string AccountName { get; set; }

    #endregion // Properties

    #region IModal

    /// <summary>
    /// Title
    /// </summary>
    public string Title => "Add or refresh a GitHub account";

    #endregion // IModal
}
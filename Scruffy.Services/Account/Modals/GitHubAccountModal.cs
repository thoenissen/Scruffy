using Discord.Interactions;

namespace Scruffy.Services.Account.Modals;

/// <summary>
/// Add or refresh a GitHub Account
/// </summary>
public class GitHubAccountModal : IModal
{
    /// <summary>
    /// Custom id
    /// </summary>
    public const string CustomId = "modal;acccount;add-or-refresh-github-account";

    /// <summary>
    /// Title
    /// </summary>
    public string Title => "Add or refresh a GitHub account";

    /// <summary>
    /// API-Key
    /// </summary>
    [InputLabel("Account name")]
    [RequiredInput(false)]
    [ModalTextInput(nameof(AccountName))]
    public string AccountName { get; set; }
}
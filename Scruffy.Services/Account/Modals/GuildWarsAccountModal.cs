using Discord;
using Discord.Interactions;

namespace Scruffy.Services.Account.Modals;

/// <summary>
/// Guild Wars 2 account
/// </summary>
public class GuildWarsAccountModal : IModal
{
    /// <summary>
    /// Custom id
    /// </summary>
    public const string CustomId = "modal;acccount;add-or-refresh-guild-wars-account";

    /// <summary>
    /// Title
    /// </summary>
    public string Title => "Add or refresh a Guild Wars 2 account";

    /// <summary>
    /// API-Key
    /// </summary>
    [InputLabel("API-Key")]
    [RequiredInput]
    [ModalTextInput(nameof(APIKey), TextInputStyle.Short, "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXXXXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX")]
    public string APIKey { get; set; }
}
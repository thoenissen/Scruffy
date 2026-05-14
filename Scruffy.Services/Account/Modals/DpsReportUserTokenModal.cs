using Discord.Interactions;

namespace Scruffy.Services.Account.Modals;

/// <summary>
/// GW2 DPS Report user token
/// </summary>
public class DpsReportUserTokenModal : IModal
{
    #region Constants

    /// <summary>
    /// Custom id
    /// </summary>
    public const string CustomId = "modal;acccount;dpsreports-account";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Title
    /// </summary>
    public string Title => "GW2 DPS Reports user token";

    /// <summary>
    /// API-Key
    /// </summary>
    [InputLabel("User token")]
    [RequiredInput(false)]
    [ModalTextInput(nameof(UserToken))]
    public string UserToken { get; set; }

    #endregion // Properties
}
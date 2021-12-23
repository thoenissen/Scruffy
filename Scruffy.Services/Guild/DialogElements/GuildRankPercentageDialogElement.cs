using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Input of the percentage quota of the rank
/// </summary>
public class GuildRankPercentageDialogElement : DialogMessageElementBase<double>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildRankPercentageDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogMessageElementBase<double>

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the quota in percent. (#.###)");

    #endregion DialogMessageElementBase<double>
}
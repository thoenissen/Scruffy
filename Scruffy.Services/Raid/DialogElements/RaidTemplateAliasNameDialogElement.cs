using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Acquisition of the template alias name
/// </summary>
public class RaidTemplateAliasNameDialogElement : DialogMessageElementBase<string>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public RaidTemplateAliasNameDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogMessageElementBase<string>

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the alias name which should be used.");

    #endregion // DialogMessageElementBase<string>
}
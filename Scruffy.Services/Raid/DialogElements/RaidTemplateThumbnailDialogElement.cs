using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Acquisition of the template thumbnail
/// </summary>
public class RaidTemplateThumbnailDialogElement : DialogMessageElementBase<string>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public RaidTemplateThumbnailDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogMessageElementBase<string>

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the link to the thumbnail which should be used.");

    /// <inheritdoc/>
    public override string ConvertMessage(IUserMessage message)
    {
        return message.Attachments?.Count > 0
                   ? message.Attachments.First().Url
                   : base.ConvertMessage(message);
    }

    #endregion // DialogMessageElementBase<string>
}
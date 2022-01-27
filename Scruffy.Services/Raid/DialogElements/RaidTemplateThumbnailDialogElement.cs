
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

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

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the link to the thumbnail which should be used.");

    /// <summary>
    /// Converting the response message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Result</returns>
    public override string ConvertMessage(DiscordMessage message)
    {
        return message.Attachments?.Count > 0
                   ? message.Attachments[0].Url
                   : base.ConvertMessage(message);
    }

    #endregion // DialogMessageElementBase<string>
}
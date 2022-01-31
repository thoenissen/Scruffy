using System.Globalization;

using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Acquisition of the special rank grant threshold
/// </summary>
public class GuildSpecialRankRemoveThresholdDialogElement : DialogMessageElementBase<double>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildSpecialRankRemoveThresholdDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogMessageElementBase<string>

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the remove threshold which should be used.");

    /// <summary>
    /// Converting the response message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Result</returns>
    public override double ConvertMessage(IUserMessage message)
    {
        return double.Parse(message.Content, NumberStyles.Any, LocalizationGroup.CultureInfo);
    }

    #endregion // DialogMessageElementBase<string>
}
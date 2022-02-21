using System.Globalization;

using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Acquisition of the custom value threshold
/// </summary>
public class GuildConfigurationItemCustomValueThresholdDialogElement : DialogMessageElementBase<int?>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildConfigurationItemCustomValueThresholdDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogMessageElementBase<string>

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the custom value threshold which can be assigned or null if you don't want to assign a threshold.");

    /// <summary>
    /// Converting the response message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Result</returns>
    public override int? ConvertMessage(IUserMessage message)
    {
        var value = int.Parse(message.Content, NumberStyles.Any, LocalizationGroup.CultureInfo);

        return value == 0
                   ? null
                   : value;
    }

    #endregion // DialogMessageElementBase<string>
}
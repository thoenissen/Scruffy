using System.Globalization;

using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Acquisition of the custom value
/// </summary>
public class GuildConfigurationItemCustomValueDialogElement : DialogMessageElementBase<long>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildConfigurationItemCustomValueDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogMessageElementBase<string>

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the custom value which can be assigned.");

    /// <inheritdoc/>
    public override long ConvertMessage(IUserMessage message)
    {
        return long.Parse(message.Content, NumberStyles.Any, LocalizationGroup.CultureInfo);
    }

    #endregion // DialogMessageElementBase<string>
}
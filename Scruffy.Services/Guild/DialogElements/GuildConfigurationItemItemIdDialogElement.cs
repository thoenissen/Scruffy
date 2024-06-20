using System.Globalization;

using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Acquisition of the item id
/// </summary>
public class GuildConfigurationItemItemIdDialogElement : DialogMessageElementBase<int>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildConfigurationItemItemIdDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogMessageElementBase<string>

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the item id of the item which should be configured.");

    /// <inheritdoc/>
    public override int ConvertMessage(IUserMessage message)
    {
        return int.Parse(message.Content, NumberStyles.Any, LocalizationGroup.CultureInfo);
    }

    #endregion // DialogMessageElementBase<string>
}
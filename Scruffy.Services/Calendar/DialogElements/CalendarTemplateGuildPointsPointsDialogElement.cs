using System.Globalization;

using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar.DialogElements;

/// <summary>
/// Acquisition of the calendar template guild points
/// </summary>
public class CalendarTemplateGuildPointsPointsDialogElement : DialogMessageElementBase<double>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public CalendarTemplateGuildPointsPointsDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogMessageElementBase<string>

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the number of guild points which can be earned by this event.");

    /// <inheritdoc/>
    public override double ConvertMessage(IUserMessage message)
    {
        return double.TryParse(message.Content, NumberStyles.Any, LocalizationGroup.CultureInfo, out var value) ? value : 0;
    }

    #endregion // DialogMessageElementBase<string>
}
using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar.DialogElements;

/// <summary>
/// Acquisition of the day of the appointment
/// </summary>
public class CalendarOneTimeDayDialogElement : DialogMessageElementBase<DateTime>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public CalendarOneTimeDayDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogMessageElementBase<string>

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the day. (yyyy-MM-dd)");

    /// <inheritdoc/>
    public override DateTime ConvertMessage(IUserMessage message)
    {
        return DateTime.TryParseExact(message.Content,
                                      "yyyy-MM-dd",
                                      null,
                                      System.Globalization.DateTimeStyles.None,
                                      out var date)
                   ? date
                   : throw new InvalidOperationException();
    }

    #endregion // DialogMessageElementBase<string>
}
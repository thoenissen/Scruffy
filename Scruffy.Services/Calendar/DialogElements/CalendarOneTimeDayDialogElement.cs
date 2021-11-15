using DSharpPlus.Entities;

using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

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

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the day. (yyyy-MM-dd)");

    /// <summary>
    /// Converting the response message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Result</returns>
    public override DateTime ConvertMessage(DiscordMessage message)
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
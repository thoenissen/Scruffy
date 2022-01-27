
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Calendar.DialogElements;

/// <summary>
/// Acquisition of the occurence count
/// </summary>
public class CalendarScheduleOccurenceCountDialogElement : DialogMessageElementBase<int>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public CalendarScheduleOccurenceCountDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogMessageElementBase<string>

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => LocalizationGroup.GetFormattedText("Message", "Please enter on which {0} of the month the appointment should be created or zero if it should be created every week.", LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(DialogContext.GetValue<DayOfWeek>("DayOfWeek")));

    /// <summary>
    /// Converting the response message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Result</returns>
    public override int ConvertMessage(DiscordMessage message)
    {
        return int.TryParse(message.Content, out var result) ? result : 0;
    }

    #endregion // DialogMessageElementBase<string>
}
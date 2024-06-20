using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

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

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetFormattedText("Message", "Please enter on which {0} of the month the appointment should be created or zero if it should be created every week.", LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(DialogContext.GetValue<DayOfWeek>("DayOfWeek")));

    /// <inheritdoc/>
    public override int ConvertMessage(IUserMessage message)
    {
        return int.TryParse(message.Content, out var result) ? result : 0;
    }

    #endregion // DialogMessageElementBase<string>
}
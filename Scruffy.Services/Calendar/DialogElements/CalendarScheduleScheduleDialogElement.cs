using Discord;

using Newtonsoft.Json;

using Scruffy.Data.Enumerations.Calendar;
using Scruffy.Data.Services.Calendar;
using Scruffy.Services.Calendar.DialogElements.Forms;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar.DialogElements;

/// <summary>
/// Creating the schedule
/// </summary>
public class CalendarScheduleScheduleDialogElement : DialogEmbedMessageElementBase<CalenderScheduleData>
{
    #region Fields

    /// <summary>
    /// Types
    /// </summary>
    private Dictionary<int, CalendarAppointmentScheduleType> _types;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public CalendarScheduleScheduleDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogEmbedMessageElementBase<long>

    /// <inheritdoc/>
    public override EmbedBuilder GetMessage()
    {
        var builder = new EmbedBuilder();
        builder.WithTitle(LocalizationGroup.GetText("ChooseTypeTitle", "Type selection"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseTypeDescription", "Please choose one of the following types:"));

        _types = [];
        var typesField = new StringBuilder();

        var i = 1;

        foreach (var type in Enum.GetValues(typeof(CalendarAppointmentScheduleType))
                                 .OfType<CalendarAppointmentScheduleType>())
        {
            typesField.Append('`');
            typesField.Append(i);
            typesField.Append("` - ");
            typesField.Append(' ');
            typesField.Append(LocalizationGroup.GetText(type.ToString(), type.ToString()));
            typesField.Append('\n');

            _types[i] = type;

            i++;
        }

        builder.AddField(LocalizationGroup.GetText("TypesField", "Types"), typesField.ToString());

        return builder;
    }

    /// <inheritdoc/>
    public override async Task<CalenderScheduleData> ConvertMessage(IUserMessage message)
    {
        var data = new CalenderScheduleData();

        if (int.TryParse(message.Content, out var index)
         && _types.TryGetValue(index, out var type))
        {
            object additionalData;

            switch (type)
            {
                case CalendarAppointmentScheduleType.WeekDayOfMonth:
                    {
                        var typeData = await RunSubForm<CreateWeekDayOfMonthForm>().ConfigureAwait(false);

                        object optionsData = null;

                        if (typeData.Options == WeekDayOfMonthSpecialOptions.MonthSelection)
                        {
                            optionsData = await RunSubElement<CalendarScheduleMonthSelectionDialogElement, List<int>>().ConfigureAwait(false);
                        }

                        additionalData = new WeekDayOfMonthData
                                         {
                                             DayOfWeek = typeData.DayOfWeek,
                                             OccurenceCount = typeData.OccurenceCount,
                                             Options = typeData.Options,
                                             OptionsData = optionsData != null ? JsonConvert.SerializeObject(optionsData) : null
                                         };
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            data.AdditionalData = JsonConvert.SerializeObject(additionalData);
        }
        else
        {
            throw new InvalidOperationException();
        }

        return data;
    }

    #endregion // DialogEmbedMessageElementBase<long>
}
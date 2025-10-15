using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar.DialogElements;

/// <summary>
/// Selection of a schedule
/// </summary>
public class CalendarScheduleSelectionDialogElement : DialogEmbedMessageElementBase<long>
{
    #region Fields

    /// <summary>
    /// Schedules
    /// </summary>
    private Dictionary<int, long> _schedules;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public CalendarScheduleSelectionDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogEmbedMessageElementBase<long>

    /// <inheritdoc/>
    public override EmbedBuilder GetMessage()
    {
        var builder = new EmbedBuilder();
        builder.WithTitle(LocalizationGroup.GetText("ChooseScheduleTitle", "Calendar schedule selection"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseScheduleDescription", "Please choose one of the following calendar schedules:"));

        _schedules = new Dictionary<int, long>();
        var levelsFieldsText = new StringBuilder();

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var mainRoles = dbFactory.GetRepository<CalendarAppointmentScheduleRepository>()
                                     .GetQuery()
                                     .Select(obj => new
                                                    {
                                                        obj.Id,
                                                        obj.Description
                                                    })
                                     .OrderBy(obj => obj.Description)
                                     .ToList();

            var i = 1;

            foreach (var role in mainRoles)
            {
                levelsFieldsText.Append('`');
                levelsFieldsText.Append(i);
                levelsFieldsText.Append("` - ");
                levelsFieldsText.Append(' ');
                levelsFieldsText.Append(role.Description);
                levelsFieldsText.Append('\n');

                _schedules[i] = role.Id;

                i++;
            }

            builder.AddField(LocalizationGroup.GetText("ScheduleField", "Schedules"), levelsFieldsText.ToString());
        }

        return builder;
    }

    /// <inheritdoc/>
    public override Task<long> ConvertMessage(IUserMessage message)
    {
        return Task.FromResult(int.TryParse(message.Content, out var index) && _schedules.TryGetValue(index, out var selectedScheduleId) ? selectedScheduleId : throw new InvalidOperationException());
    }

    #endregion // DialogEmbedMessageElementBase<long>
}
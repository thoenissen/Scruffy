using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar.DialogElements;

/// <summary>
/// Selection of an appointment
/// </summary>
public class CalendarAppointmentSelectionDialogElement : DialogEmbedSelectMenuElementBase<long>
{
    #region Fields

    /// <summary>
    /// Appointments
    /// </summary>
    private List<SelectMenuEntryData<long>> _appointments;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public CalendarAppointmentSelectionDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogEmbedMessageElementBase<long>

    /// <inheritdoc/>
    public override Task<EmbedBuilder> GetMessage()
    {
        return Task.FromResult(new EmbedBuilder().WithTitle(LocalizationGroup.GetText("ChooseTitle", "Appointment selection"))
                                                 .WithDescription(LocalizationGroup.GetText("ChooseDescription", "Please choose one of the following appointments:"))
                                                 .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                                 .WithColor(Color.Green)
                                                 .WithTimestamp(DateTime.Now));
    }

    /// <inheritdoc/>
    protected override long DefaultFunc() => 0;

    /// <inheritdoc/>
    public override IReadOnlyList<SelectMenuEntryData<long>> GetEntries()
    {
        if (_appointments == null)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var limit = DateTime.Now.AddMinutes(15);

                var appointments = dbFactory.GetRepository<CalendarAppointmentRepository>()
                                            .GetQuery()
                                            .Where(obj => obj.TimeStamp < limit)
                                            .Select(obj => new
                                                           {
                                                               obj.Id,
                                                               obj.TimeStamp,
                                                               obj.CalendarAppointmentTemplate.Description
                                                           })
                                            .OrderByDescending(obj => obj.TimeStamp)
                                            .Take(10)
                                            .ToList();

                _appointments = appointments.Select(obj => new SelectMenuEntryData<long>
                                                           {
                                                               CommandText = $"{obj.Description} - {obj.TimeStamp.ToString("g", LocalizationGroup.CultureInfo)}",
                                                               Response = () => Task.FromResult(obj.Id)
                                                           })
                                            .ToList();
            }
        }

        return _appointments;
    }

    #endregion // DialogEmbedMessageElementBase<long>
}
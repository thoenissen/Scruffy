using System.Diagnostics;

using Discord;
using Discord.WebSocket;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Tables.Calendar;
using Scruffy.Data.Enumerations.Calendar;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Data.Services.Calendar;
using Scruffy.Services.Calendar.DialogElements;
using Scruffy.Services.Calendar.DialogElements.Forms;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.Services.Calendar;

/// <summary>
/// Calendar schedule service
/// </summary>
public class CalendarScheduleService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Message builder
    /// </summary>
    private readonly CalendarMessageBuilderService _messageBuilder;

    /// <summary>
    /// Discord client
    /// </summary>
    private readonly DiscordSocketClient _discordClient;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="messageBuilder">Message builder</param>
    /// <param name="discordClient">Discord client</param>
    public CalendarScheduleService(LocalizationService localizationService,
                                   CalendarMessageBuilderService messageBuilder,
                                   DiscordSocketClient discordClient)
        : base(localizationService)
    {
        _messageBuilder = messageBuilder;
        _discordClient = discordClient;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Managing the Schedules
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RunAssistantAsync(IContextContainer commandContext)
    {
        bool repeat;

        do
        {
            repeat = await DialogHandler.Run<CalendarScheduleSetupDialogElement, bool>(commandContext).ConfigureAwait(false);
        }
        while (repeat);
    }

    /// <summary>
    /// Create new appointments
    /// </summary>
    /// <param name="serverId">Id of the server</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CreateAppointments(ulong? serverId)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            foreach (var schedule in await dbFactory.GetRepository<CalendarAppointmentScheduleRepository>()
                                                    .GetQuery()
                                                    .Where(obj => obj.IsDeleted == false
                                                               && (serverId == null || obj.CalendarAppointmentTemplate.DiscordServerId == serverId))
                                                    .Select(obj => new
                                                                   {
                                                                       obj.Id,
                                                                       obj.Type,
                                                                       obj.AdditionalData,
                                                                       obj.CalendarAppointmentTemplateId,
                                                                       obj.CalendarAppointmentTemplate.AppointmentTime
                                                                   })
                                                    .ToListAsync()
                                                    .ConfigureAwait(false))
            {
                switch (schedule.Type)
                {
                    case CalendarAppointmentScheduleType.WeekDayOfMonth:
                        {
                            var additionalData = JsonConvert.DeserializeObject<WeekDayOfMonthData>(schedule.AdditionalData);

                            var currentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(4);

                            bool CheckOptions(DateTime timeStamp)
                            {
                                var isValid = true;

                                switch (additionalData.Options)
                                {
                                    case WeekDayOfMonthSpecialOptions.EvenMonth:
                                        {
                                            isValid = timeStamp.Month % 2 == 0;
                                        }
                                        break;

                                    case WeekDayOfMonthSpecialOptions.UnevenMonth:
                                        {
                                            isValid = timeStamp.Month % 2 == 1;
                                        }
                                        break;

                                    case WeekDayOfMonthSpecialOptions.MonthSelection:
                                        {
                                            isValid = additionalData.OptionsData != null && JsonConvert.DeserializeObject<List<int>>(additionalData.OptionsData)?.Contains(timeStamp.Month) == true;
                                        }
                                        break;
                                }

                                return isValid;
                            }

                            var difference = additionalData.DayOfWeek - currentDate.DayOfWeek;

                            currentDate = difference < 0
                                              ? currentDate.AddDays(difference + 7)
                                              : currentDate.AddDays(difference);

                            if (additionalData.OccurenceCount == 0)
                            {
                                while (currentDate < endDate)
                                {
                                    var appointmentTimeStamp = currentDate.Add(schedule.AppointmentTime);

                                    if (CheckOptions(appointmentTimeStamp))
                                    {
                                        dbFactory.GetRepository<CalendarAppointmentRepository>()
                                                 .AddOrRefresh(obj => obj.TimeStamp == appointmentTimeStamp
                                                                   && obj.CalendarAppointmentScheduleId == schedule.Id,
                                                               obj =>
                                                               {
                                                                   obj.TimeStamp = appointmentTimeStamp;
                                                                   obj.CalendarAppointmentTemplateId = schedule.CalendarAppointmentTemplateId;
                                                                   obj.CalendarAppointmentScheduleId = schedule.Id;
                                                               });
                                    }

                                    currentDate = currentDate.AddDays(7);
                                }
                            }
                            else
                            {
                                while (currentDate < endDate)
                                {
                                    currentDate = currentDate.AddDays(7 * (additionalData.OccurenceCount - 1));

                                    var appointmentTimeStamp = currentDate.Add(schedule.AppointmentTime);

                                    if (CheckOptions(appointmentTimeStamp))
                                    {
                                        dbFactory.GetRepository<CalendarAppointmentRepository>()
                                                 .AddOrRefresh(obj => obj.TimeStamp == appointmentTimeStamp
                                                                   && obj.CalendarAppointmentScheduleId == schedule.Id,
                                                               obj =>
                                                               {
                                                                   obj.TimeStamp = appointmentTimeStamp;
                                                                   obj.CalendarAppointmentTemplateId = schedule.CalendarAppointmentTemplateId;
                                                                   obj.CalendarAppointmentScheduleId = schedule.Id;
                                                               });
                                    }

                                    var nextDate = currentDate;
                                    while (nextDate.Month == currentDate.Month)
                                    {
                                        nextDate = nextDate.AddDays(7);
                                    }

                                    currentDate = nextDate;
                                }
                            }
                        }
                        break;
                    default:
                        {
                            Trace.WriteLine($"Invalid schedule: {schedule.Type}");
                        }
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Create events
    /// </summary>
    /// <param name="serverId">Id of the discord server</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CreateEvents(ulong? serverId)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var now = DateTime.Now;

            var templates = dbFactory.GetRepository<CalendarAppointmentTemplateRepository>()
                                     .GetQuery()
                                     .Select(obj => obj);

            foreach (var calendar in dbFactory.GetRepository<GuildRepository>()
                                              .GetQuery()
                                              .Where(obj => serverId == null || obj.DiscordServerId == serverId)
                                              .Select(obj => new
                                                             {
                                                                 ServerId = obj.DiscordServerId,
                                                                 Appointments = templates.Where(obj2 => obj2.DiscordServerId == obj.DiscordServerId
                                                                                                     && obj2.DiscordVoiceChannel != null)
                                                                                         .SelectMany(obj2 => obj2.CalendarAppointments
                                                                                                                 .Where(obj3 => obj3.TimeStamp > now
                                                                                                                             && obj2.CalendarAppointments.Any(obj4 => obj4.TimeStamp > now
                                                                                                                                                                   && obj4.TimeStamp < obj3.TimeStamp) == false)
                                                                                                                 .Select(obj3 => new
                                                                                                                 {
                                                                                                                     obj3.Id,
                                                                                                                     obj3.TimeStamp,
                                                                                                                     obj3.DiscordEventId,
                                                                                                                     obj2.Description,
                                                                                                                     obj2.DiscordVoiceChannel,
                                                                                                                     obj2.DiscordEventDescription,
                                                                                                                 }))
                                                                                         .Where(obj3 => obj3.DiscordEventId == null)
                                                                                         .OrderBy(obj2 => obj2.TimeStamp)
                                                                                         .ToList()
                                                             })
                                              .ToList())
            {
                var server = _discordClient.GetGuild(calendar.ServerId);
                if (server != null)
                {
                    foreach (var appointment in calendar.Appointments)
                    {
                        var guildEvent = await server.CreateEventAsync(appointment.Description,
                                                                       appointment.TimeStamp.ToUniversalTime(),
                                                                       GuildScheduledEventType.Voice,
                                                                       GuildScheduledEventPrivacyLevel.Private,
                                                                       appointment.DiscordEventDescription,
                                                                       null,
                                                                       appointment.DiscordVoiceChannel)
                                                     .ConfigureAwait(false);

                        dbFactory.GetRepository<CalendarAppointmentRepository>()
                                 .Refresh(obj => obj.Id == appointment.Id,
                                          obj => obj.DiscordEventId = guildEvent.Id);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Adding a one time event
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task AddOneTimeEvent(IContextContainer commandContext)
    {
        var data = await DialogHandler.RunForm<CreateOneTimeEventFormData>(commandContext, false)
                                      .ConfigureAwait(false);
        if (data != null)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var appointmentTime = dbFactory.GetRepository<CalendarAppointmentTemplateRepository>()
                                               .GetQuery()
                                               .Where(obj => obj.Id == data.TemplateId)
                                               .Select(obj => obj.AppointmentTime)
                                               .First();

                if (dbFactory.GetRepository<CalendarAppointmentRepository>()
                             .Add(new CalendarAppointmentEntity
                                  {
                                      CalendarAppointmentScheduleId = null,
                                      CalendarAppointmentTemplateId = data.TemplateId,
                                      TimeStamp = data.Day.Add(appointmentTime)
                                  }))
                {
                    await _messageBuilder.RefreshMessages(commandContext.Guild.Id)
                                         .ConfigureAwait(false);
                }
            }
        }
    }

    #endregion // Methods
}
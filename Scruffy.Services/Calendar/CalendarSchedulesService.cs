using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Data.Enumerations.Calendar;
using Scruffy.Data.Services.Calendar;
using Scruffy.Services.Calendar.DialogElements;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;

namespace Scruffy.Services.Calendar
{
    /// <summary>
    /// Calendar schedule service
    /// </summary>
    public class CalendarScheduleService : LocatedServiceBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public CalendarScheduleService(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Managing the Schedules
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RunAssistantAsync(CommandContext commandContext)
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
                                                                   && (serverId == null || obj.CalendarAppointmentTemplate.ServerId == serverId))
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
                                var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month + 3, 1);

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
                                        while (nextDate.Month <= currentDate.Month)
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

        #endregion // Methods
    }
}
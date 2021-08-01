using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Data.Entity.Repositories.GuildAdministration;
using Scruffy.Services.Core;

namespace Scruffy.Services.Calendar
{
    /// <summary>
    /// Refreshing messages of the calendar
    /// </summary>
    public class CalendarMessageBuilderService : LocatedServiceBase
    {
        #region Fields

        /// <summary>
        /// Client
        /// </summary>
        private DiscordClient _discordClient;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="discordClient">Discord client</param>
        public CalendarMessageBuilderService(LocalizationService localizationService, DiscordClient discordClient)
            : base(localizationService)
        {
            _discordClient = discordClient;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Refreshing all calendars
        /// </summary>
        /// <param name="serverId">Id of the server</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RefreshMotds(ulong? serverId)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var now = DateTime.Now;

                var appointments = dbFactory.GetRepository<CalendarAppointmentRepository>()
                                            .GetQuery()
                                            .Where(obj => obj.TimeStamp > now);

                foreach (var calendar in dbFactory.GetRepository<GuildRepository>()
                                                  .GetQuery()
                                                  .Where(obj => obj.MessageOfTheDayChannelId != null
                                                             && obj.MessageOfTheDayMessageId != null
                                                             && (serverId == null || obj.DiscordServerId == serverId))
                                                  .Select(obj => new
                                                  {
                                                      obj.CalendarTitle,
                                                      obj.CalendarDescription,
                                                      obj.MessageOfTheDayChannelId,
                                                      obj.MessageOfTheDayMessageId,
                                                      Appointments = appointments.Where(obj2 => obj2.CalendarAppointmentTemplate.ServerId == obj.DiscordServerId)
                                                                                                .Select(obj2 => new
                                                                                                {
                                                                                                    obj2.TimeStamp,
                                                                                                    obj2.CalendarAppointmentTemplate.Description,
                                                                                                })
                                                                                                .OrderBy(obj2 => obj2.TimeStamp)
                                                                                                .ToList()
                                                  }))
                {
                    var messageBuilder = new StringBuilder();

                    messageBuilder.AppendLine("--------------------");

                    foreach (var appointment in calendar.Appointments)
                    {
                        var currentLine = LocalizationGroup.GetFormattedText("MotdFormat",
                                                                             "{0} - {1}, {2:dd.MM} at {2:hh:mm}",
                                                                             appointment.Description,
                                                                             LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(appointment.TimeStamp.DayOfWeek),
                                                                             appointment.TimeStamp);

                        if (messageBuilder.Length + currentLine.Length > 300)
                        {
                            break;
                        }

                        messageBuilder.AppendLine(currentLine);
                    }

                    messageBuilder.Append("--------------------");

                    var channel = await _discordClient.GetChannelAsync(calendar.MessageOfTheDayChannelId.Value)
                                                      .ConfigureAwait(false);

                    if (channel != null)
                    {
                        var message = await channel.GetMessageAsync(calendar.MessageOfTheDayMessageId.Value)
                                                   .ConfigureAwait(false);

                        if (message != null)
                        {
                            await message.ModifyAsync(Formatter.Bold(LocalizationGroup.GetText("Motd", "Message of the day:")) + "\n" + Formatter.BlockCode(messageBuilder.ToString()))
                                         .ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Refreshing all calendars
        /// </summary>
        /// <param name="serverId">Id of the server</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RefreshMessages(ulong? serverId)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var now = DateTime.Now;

                var appointments = dbFactory.GetRepository<CalendarAppointmentRepository>()
                                            .GetQuery()
                                            .Where(obj => obj.TimeStamp > now);

                foreach (var calendar in dbFactory.GetRepository<GuildRepository>()
                                                  .GetQuery()
                                                  .Where(obj => obj.GuildCalendarChannelId != null
                                                             && obj.GuildCalendarMessageId != null
                                                             && (serverId == null || obj.DiscordServerId == serverId))
                                                  .Select(obj => new
                                                                 {
                                                                     obj.CalendarTitle,
                                                                     obj.CalendarDescription,
                                                                     obj.GuildCalendarChannelId,
                                                                     obj.GuildCalendarMessageId,
                                                                     Appointments = appointments.Where(obj2 => obj2.CalendarAppointmentTemplate.ServerId == obj.DiscordServerId)
                                                                                                .Select(obj2 => new
                                                                                                                {
                                                                                                                    obj2.TimeStamp,
                                                                                                                    obj2.CalendarAppointmentTemplate.Description,
                                                                                                                    obj2.CalendarAppointmentTemplate.Uri
                                                                                                                })
                                                                                                .OrderBy(obj2 => obj2.TimeStamp)
                                                                                                .ToList()
                                                                 }))
                {
                    var builder = new DiscordEmbedBuilder();

                    builder.WithTitle(calendar.CalendarTitle);
                    builder.WithDescription(calendar.CalendarDescription);
                    builder.WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/836238701046398987/d7d1b509a23aa9789885127da9107fe0.png?size=256");
                    builder.WithThumbnail("https://cdn.discordapp.com/attachments/847555191842537552/870776562943946782/12382200801557740332-512.png");
                    builder.WithColor(DiscordColor.Green);
                    builder.WithTimestamp(DateTime.Now);

                    if (calendar.Appointments.Count > 0)
                    {
                        var first = calendar.Appointments.First();

                        var currentMonth = first.TimeStamp.Month;
                        var currentWeekOfYear = GetIso8601WeekOfYear(first.TimeStamp);
                        var currentFieldTitle = $"{LocalizationGroup.CultureInfo.DateTimeFormat.GetMonthName(first.TimeStamp.Month)} {first.TimeStamp.Year}\n\n{LocalizationGroup.GetText("WeekNumber", "Week")} {GetIso8601WeekOfYear(first.TimeStamp)}";

                        var fieldCounter = 0;
                        var stringBuilder = new StringBuilder();

                        foreach (var appointment in calendar.Appointments)
                        {
                            var currentLine = $@"`({LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(appointment.TimeStamp.DayOfWeek).Substring(0, 2)}) {appointment.TimeStamp.ToString("g", LocalizationGroup.CultureInfo)}` {(string.IsNullOrWhiteSpace(appointment.Uri) ? appointment.Description : Formatter.MaskedUrl(appointment.Description, new Uri(appointment.Uri)))}";

                            if (currentMonth != appointment.TimeStamp.Month
                             || currentWeekOfYear != GetIso8601WeekOfYear(appointment.TimeStamp)
                             || stringBuilder.Length + currentLine.Length > 1024)
                            {
                                builder.AddField(currentFieldTitle, stringBuilder.ToString());

                                fieldCounter++;

                                if (fieldCounter >= 6)
                                {
                                    break;
                                }

                                stringBuilder.Clear();
                                stringBuilder.AppendLine(currentLine);

                                currentFieldTitle = currentWeekOfYear == GetIso8601WeekOfYear(appointment.TimeStamp)
                                                        ? "\u200B"
                                                        : currentMonth != appointment.TimeStamp.Month
                                                            ? $"{LocalizationGroup.CultureInfo.DateTimeFormat.GetMonthName(appointment.TimeStamp.Month)} {appointment.TimeStamp.Year}\n\n{LocalizationGroup.GetText("WeekNumber", "Week")} {GetIso8601WeekOfYear(appointment.TimeStamp)}"
                                                            : $"{LocalizationGroup.GetText("WeekNumber", "Week")} {GetIso8601WeekOfYear(appointment.TimeStamp)}";
                            }
                            else
                            {
                                stringBuilder.AppendLine(currentLine);
                            }

                            currentMonth = appointment.TimeStamp.Month;
                            currentWeekOfYear = GetIso8601WeekOfYear(appointment.TimeStamp);
                        }

                        if (fieldCounter < 6
                         && stringBuilder.Length > 0)
                        {
                            builder.AddField(currentFieldTitle, stringBuilder.ToString());
                        }
                    }

                    var channel = await _discordClient.GetChannelAsync(calendar.GuildCalendarChannelId.Value)
                                                      .ConfigureAwait(false);

                    if (channel != null)
                    {
                        var message = await channel.GetMessageAsync(calendar.GuildCalendarMessageId.Value)
                                                   .ConfigureAwait(false);

                        if (message != null)
                        {
                            await message.ModifyAsync(null, builder.Build())
                                         .ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returning ISO 8601 week of year
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>Week number</returns>
        private static int GetIso8601WeekOfYear(DateTime date)
        {
            var day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                date = date.AddDays(3);
            }

            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        #endregion // Methods
    }
}

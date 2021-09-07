using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Data.Entity.Repositories.GuildAdministration;
using Scruffy.Data.Enumerations.GuildAdministration;
using Scruffy.Data.Json.Calendar;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Extensions;

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

                var templates = dbFactory.GetRepository<CalendarAppointmentTemplateRepository>()
                                         .GetQuery()
                                         .Select(obj => obj);

                var channels = dbFactory.GetRepository<GuildChannelConfigurationRepository>()
                                        .GetQuery()
                                        .Select(obj => obj);

                foreach (var calendar in dbFactory.GetRepository<GuildRepository>()
                                                  .GetQuery()
                                                  .Where(obj => serverId == null || obj.DiscordServerId == serverId)
                                                  .Select(obj => new
                                                                 {
                                                                     Channel = channels.Where(obj2 => obj2.GuildId == obj.Id
                                                                                                      && obj2.Type == GuildChannelConfigurationType.CalendarMessageOfTheDay)
                                                                                       .Select(obj2 => new
                                                                                                       {
                                                                                                           obj2.ChannelId,
                                                                                                           obj2.MessageId
                                                                                                       })
                                                                                       .FirstOrDefault(),
                                                                     Appointments = templates.Where(obj2 => obj2.ServerId == obj.DiscordServerId)
                                                                                             .SelectMany(obj2 => obj2.CalendarAppointments
                                                                                                                     .Where(obj3 => obj3.TimeStamp > now
                                                                                                                                 && obj2.CalendarAppointments.Any(obj4 => obj4.TimeStamp > now
                                                                                                                                                                               && obj4.TimeStamp < obj3.TimeStamp) == false)
                                                                                                                     .Select(obj3 => new
                                                                                                                                     {
                                                                                                                                         obj3.TimeStamp,
                                                                                                                                         obj2.Description
                                                                                                                                     }))
                                                                                             .OrderBy(obj2 => obj2.TimeStamp)
                                                                                             .ToList()
                                                                 })
                                                  .Where(obj => obj.Channel.ChannelId > 0)
                                                  .ToList())
                {
                    var messageBuilder = new StringBuilder();

                    messageBuilder.AppendLine("--------------------");

                    foreach (var appointment in calendar.Appointments)
                    {
                        var currentLine = LocalizationGroup.GetFormattedText("MotdFormat",
                                                                             "{0} - {1}, {2:dd.MM.} at {2:HH:mm}",
                                                                             appointment.Description,
                                                                             LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(appointment.TimeStamp.DayOfWeek),
                                                                             appointment.TimeStamp);

                        if (messageBuilder.Length + currentLine.Length > 900)
                        {
                            break;
                        }

                        messageBuilder.AppendLine(currentLine);
                    }

                    messageBuilder.Append("--------------------");

                    var channel = await _discordClient.GetChannelAsync(calendar.Channel.ChannelId)
                                                      .ConfigureAwait(false);

                    if (channel != null)
                    {
                        var message = await channel.GetMessageAsync(calendar.Channel.MessageId.Value)
                                                   .ConfigureAwait(false);

                        if (message != null)
                        {
                            await message.ModifyAsync(null,
                                                      new DiscordEmbedBuilder().WithTitle(LocalizationGroup.GetText("Motd", "Message of the day:"))
                                                                               .WithDescription(Formatter.BlockCode(messageBuilder.ToString()))
                                                                               .WithColor(DiscordColor.Green)
                                                                               .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/ef1f3e1f3f40100fb3750f8d7d25c657.png?size=64")
                                                                               .WithTimestamp(DateTime.Now)
                                                                               .Build())
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

                var channels = dbFactory.GetRepository<GuildChannelConfigurationRepository>()
                                        .GetQuery()
                                        .Select(obj => obj);

                foreach (var calendar in dbFactory.GetRepository<GuildRepository>()
                                                  .GetQuery()
                                                  .Where(obj => serverId == null || obj.DiscordServerId == serverId)
                                                  .Select(obj => new
                                                                 {
                                                                     Channel = channels.Where(obj2 => obj2.GuildId == obj.Id
                                                                                                      && obj2.Type == GuildChannelConfigurationType.CalendarOverview)
                                                                                       .Select(obj2 => new
                                                                                                       {
                                                                                                           obj2.ChannelId,
                                                                                                           obj2.MessageId,
                                                                                                           obj2.AdditionalData
                                                                                                       })
                                                                                       .FirstOrDefault(),
                                                                     Appointments = appointments.Where(obj2 => obj2.CalendarAppointmentTemplate.ServerId == obj.DiscordServerId)
                                                                                                .Select(obj2 => new
                                                                                                                {
                                                                                                                    obj2.TimeStamp,
                                                                                                                    obj2.CalendarAppointmentTemplate.Description,
                                                                                                                    obj2.CalendarAppointmentTemplate.Uri
                                                                                                                })
                                                                                                .OrderBy(obj2 => obj2.TimeStamp)
                                                                                                .ToList()
                                                                 })
                                                  .Where(obj => obj.Channel.ChannelId > 0)
                                                  .ToList())
                {
                    var builder = new DiscordEmbedBuilder();

                    var additionalData = JsonConvert.DeserializeObject<AdditionalCalendarChannelData>(calendar.Channel.AdditionalData);

                    builder.WithTitle(additionalData.Title);
                    builder.WithDescription(additionalData.Description);
                    builder.WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/836238701046398987/d7d1b509a23aa9789885127da9107fe0.png?size=256");
                    builder.WithThumbnail("https://cdn.discordapp.com/attachments/847555191842537552/870776562943946782/12382200801557740332-512.png");
                    builder.WithColor(DiscordColor.Green);
                    builder.WithTimestamp(DateTime.Now);

                    if (calendar.Appointments.Count > 0)
                    {
                        var first = calendar.Appointments.First();

                        var currentMonth = first.TimeStamp.Month;
                        var currentWeekOfYear = first.TimeStamp.GetIso8601WeekOfYear();
                        var currentFieldTitle = $"{LocalizationGroup.CultureInfo.DateTimeFormat.GetMonthName(first.TimeStamp.Month)} {first.TimeStamp.Year}\n\n{LocalizationGroup.GetText("WeekNumber", "Week")} {first.TimeStamp.GetIso8601WeekOfYear()}";

                        var fieldCounter = 0;
                        var stringBuilder = new StringBuilder();

                        foreach (var appointment in calendar.Appointments)
                        {
                            var currentLine = $@"`({LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(appointment.TimeStamp.DayOfWeek).Substring(0, 2)}) {appointment.TimeStamp.ToString("g", LocalizationGroup.CultureInfo)}` {(string.IsNullOrWhiteSpace(appointment.Uri) ? appointment.Description : Formatter.MaskedUrl(appointment.Description, new Uri(appointment.Uri)))}";

                            if (currentMonth != appointment.TimeStamp.Month
                             || currentWeekOfYear != appointment.TimeStamp.GetIso8601WeekOfYear()
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

                                currentFieldTitle = currentMonth != appointment.TimeStamp.Month
                                                        ? $"{LocalizationGroup.CultureInfo.DateTimeFormat.GetMonthName(appointment.TimeStamp.Month)} {appointment.TimeStamp.Year}\n\n{LocalizationGroup.GetText("WeekNumber", "Week")} {appointment.TimeStamp.GetIso8601WeekOfYear()}"
                                                        : currentWeekOfYear == appointment.TimeStamp.GetIso8601WeekOfYear()
                                                            ? "\u200B"
                                                            : $"{LocalizationGroup.GetText("WeekNumber", "Week")} {appointment.TimeStamp.GetIso8601WeekOfYear()}";
                            }
                            else
                            {
                                stringBuilder.AppendLine(currentLine);
                            }

                            currentMonth = appointment.TimeStamp.Month;
                            currentWeekOfYear = appointment.TimeStamp.GetIso8601WeekOfYear();
                        }

                        if (fieldCounter < 6
                         && stringBuilder.Length > 0)
                        {
                            builder.AddField(currentFieldTitle, stringBuilder.ToString());
                        }
                    }

                    var channel = await _discordClient.GetChannelAsync(calendar.Channel.ChannelId)
                                                      .ConfigureAwait(false);

                    if (channel != null)
                    {
                        var message = await channel.GetMessageAsync(calendar.Channel.MessageId.Value)
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

        #endregion // Methods
    }
}

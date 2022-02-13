using Discord;
using Discord.WebSocket;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Fractals;
using Scruffy.Data.Services.Fractal;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Fractals;

/// <summary>
/// Building the lfg message
/// </summary>
public class FractalLfgMessageBuilder : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Discord client
    /// </summary>
    private DiscordSocketClient _client;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <param name="localizationService">Localization service</param>
    public FractalLfgMessageBuilder(DiscordSocketClient client, LocalizationService localizationService)
        : base(localizationService)
    {
        _client = client;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Refresh the message
    /// </summary>
    /// <param name="configurationId">Id of the configuration</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RefreshMessageAsync(long configurationId)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var from = DateTime.Today;

            var data = await dbFactory.GetRepository<FractalLfgConfigurationRepository>()
                                      .GetQuery()
                                      .Where(obj => obj.Id == configurationId)
                                      .Select(obj => new
                                                     {
                                                         ChannelId = obj.DiscordChannelId,
                                                         MessageId = obj.DiscordMessageId,
                                                         obj.Title,
                                                         obj.Description,
                                                         Registrations = obj.FractalRegistrations
                                                                            .Select(obj2 => new
                                                                                            {
                                                                                                obj2.AppointmentTimeStamp,
                                                                                                UserId = obj2.User
                                                                                                             .DiscordAccounts
                                                                                                             .Select(obj3 => obj3.Id)
                                                                                                             .FirstOrDefault(),
                                                                                                obj2.RegistrationTimeStamp,
                                                                                            })
                                                                            .Where(obj2 => obj2.AppointmentTimeStamp > from)
                                                     })
                                      .FirstOrDefaultAsync().ConfigureAwait(false);

            if (data != null)
            {
                var registrations = data.Registrations
                                        .GroupBy(obj2 => obj2.AppointmentTimeStamp)
                                        .GroupBy(obj2 => obj2.Key.Date)
                                        .ToList();

                var messageBuilder = new EmbedBuilder
                                     {
                                         Title = data.Title,
                                         Color = Color.Green,
                                         Description = data.Description
                                     };

                var stringBuilder = new StringBuilder(500);

                for (var i = 0; i < 8; i++)
                {
                    var date = DateTime.Today.AddDays(i);
                    var name = i == 0
                                   ? $"`{i} - {LocalizationGroup.GetText("Today", "Today")}`"
                                   : $"`{i} - {LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(date.DayOfWeek)[..2]}:` {date.ToString("d", LocalizationGroup.CultureInfo)}";

                    stringBuilder.Clear();

                    var registrationsOfDay = registrations.FirstOrDefault(obj => obj.Key == date);
                    if (registrationsOfDay?.Any() == true)
                    {
                        foreach (var group in registrationsOfDay)
                        {
                            stringBuilder.Append($"> ● {group.Key.ToString("t", LocalizationGroup.CultureInfo)}\n");

                            foreach (var entry in group.OrderBy(obj => obj.RegistrationTimeStamp))
                            {
                                stringBuilder.Append($"> • {(await _client.GetUserAsync(entry.UserId).ConfigureAwait(false)).Mention}\n");
                            }
                        }

                        stringBuilder.Append("\u200B");
                    }
                    else
                    {
                        stringBuilder.Append("> ●\n\u200B");
                    }

                    messageBuilder.AddField(name, stringBuilder.ToString());
                }

                messageBuilder.WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64");
                messageBuilder.WithTimestamp(DateTime.Now);

                var channel = await _client.GetChannelAsync(data.ChannelId).ConfigureAwait(false);
                if (channel is IMessageChannel messageChannel)
                {
                    var message = await messageChannel.GetMessageAsync(data.MessageId).ConfigureAwait(false);
                    if (message is IUserMessage userMessage)
                    {
                        await userMessage.ModifyAsync(obj => obj.Embed = messageBuilder.Build()).ConfigureAwait(false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates an appointment
    /// </summary>
    /// <param name="channelId">Id of the channel</param>
    /// <param name="appointmentTimeStamp">Appointment timestamp</param>
    /// <param name="registrations">Registrations</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<ulong> CreateAppointmentMessage(ulong channelId, DateTime appointmentTimeStamp, IEnumerable<AppointmentCreationRegistrationData> registrations)
    {
        var channel = await _client.GetChannelAsync(channelId)
                                   .ConfigureAwait(false);

        if (channel is IMessageChannel messageChannel)
        {
            var builder = new StringBuilder();
            builder.Append(LocalizationGroup.GetFormattedText("AppointmentReminder", "{0} - Appointment reminder", appointmentTimeStamp.ToString("g", LocalizationGroup.CultureInfo)));
            builder.Append('\n');

            foreach (var registration in registrations)
            {
                builder.Append($"> ● {(await _client.GetUserAsync(registration.DiscordAccountId).ConfigureAwait(false)).Mention}\n");
            }

            return (await messageChannel.SendMessageAsync(builder.ToString()).ConfigureAwait(false)).Id;
        }

        throw new InvalidOperationException();
    }

    #endregion // Methods
}
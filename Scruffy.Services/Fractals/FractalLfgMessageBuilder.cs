using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Fractals;

namespace Scruffy.Services.Fractals
{
    /// <summary>
    /// Building the lfg message
    /// </summary>
    public class FractalLfgMessageBuilder
    {
        #region Fields

        /// <summary>
        /// Discord client
        /// </summary>
        private DiscordClient _client;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">Discord client</param>
        public FractalLfgMessageBuilder(DiscordClient client)
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
                var from = DateTime.Today.ToUniversalTime();

                var data = await dbFactory.GetRepository<FractalLfgConfigurationRepository>()
                                          .GetQuery()
                                          .Where(obj => obj.Id == configurationId)
                                          .Select(obj => new
                                                         {
                                                             obj.ChannelId,
                                                             obj.MessageId,
                                                             obj.Title,
                                                             obj.Description,
                                                             Registrations = obj.FractalRegistrations
                                                                                .Select(obj2 => new
                                                                                                {
                                                                                                    obj2.AppointmentTimeStamp,
                                                                                                    obj2.UserId,
                                                                                                    obj2.RegistrationTimeStamp,
                                                                                                })
                                                                                .Where(obj2 => obj2.AppointmentTimeStamp > from)
                                                         })
                                          .FirstOrDefaultAsync();

                if (data != null)
                {
                    var registrations = data.Registrations
                                            .GroupBy(obj2 => obj2.AppointmentTimeStamp)
                                            .GroupBy(obj2 => obj2.Key.Date)
                                            .ToList();

                    var messageBuilder = new DiscordEmbedBuilder
                                         {
                                             Title = data.Title,
                                             Color = DiscordColor.Green,
                                             Description = data.Description
                                         };

                    var stringBuilder = new StringBuilder(500);

                    for (var i = 0; i < 8; i++)
                    {
                        var date = DateTime.Today.AddDays(i);
                        var name = i == 0
                                       ? "Today"
                                       : $"`{i} - {DateTimeFormatInfo.CurrentInfo.GetDayName(date.DayOfWeek)[..2]}:` {date:d}";

                        stringBuilder.Clear();

                        var registrationsOfDay = registrations.FirstOrDefault(obj => obj.Key == date);
                        if (registrationsOfDay?.Any() == true)
                        {
                            foreach (var group in registrationsOfDay)
                            {
                                stringBuilder.Append($"> ● {group.Key}\n");

                                foreach (var entry in group.OrderBy(obj => obj.RegistrationTimeStamp))
                                {
                                    stringBuilder.Append($"> • {(await _client.GetUserAsync(entry.UserId)).Mention}\n");
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

                    messageBuilder.WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/836238701046398987/d7d1b509a23aa9789885127da9107fe0.png?size=256");
                    messageBuilder.WithTimestamp(DateTime.Now);

                    var channel = await _client.GetChannelAsync(data.ChannelId);
                    if (channel != null)
                    {
                        var message = await channel.GetMessageAsync(data.MessageId);

                        await message.ModifyAsync(null, messageBuilder.Build());
                    }
                }
            }
        }

        #endregion // Methods
    }
}

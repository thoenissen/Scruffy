using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Services.Raid;
using Scruffy.Services.Core;

namespace Scruffy.Services.Raid
{
    /// <summary>
    /// Building the lfg message
    /// </summary>
    public class RaidMessageBuilder : LocatedServiceBase
    {
        #region Fields

        /// <summary>
        /// Discord client
        /// </summary>
        private DiscordClient _client;

        /// <summary>
        /// Experience levels
        /// </summary>
        private List<RaidExperienceLevelData> _unorderedLevels;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">Discord client</param>
        /// <param name="localizationService">Localization service</param>
        public RaidMessageBuilder(DiscordClient client, LocalizationService localizationService)
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
                var now = DateTime.Now;

                var appointment = dbFactory.GetRepository<RaidAppointmentRepository>()
                                           .GetQuery()
                                           .Where(obj => obj.ConfigurationId == configurationId
                                                         && obj.TimeStamp > now)
                                           .OrderByDescending(obj => obj.TimeStamp)
                                           .Select(obj => new RaidAppointmentMessageData
                                                          {
                                                              TimeStamp = obj.TimeStamp,

                                                              ChannelId = obj.RaidDayConfiguration.ChannelId,
                                                              MessageId = obj.RaidDayConfiguration.MessageId,

                                                              Thumbnail = obj.RaidDayTemplateEntity.Thumbnail,
                                                              Title = obj.RaidDayTemplateEntity.Title,
                                                              Description = obj.RaidDayTemplateEntity.Description,

                                                              ExperienceLevels  = obj.RaidDayTemplateEntity
                                                                                     .RaidExperienceAssignments
                                                                                     .Select(obj2 => new RaidAppointmentMessageExperienceLevel
                                                                                                     {
                                                                                                         Id = obj2.RaidExperienceLevel.Id,
                                                                                                         DiscordEmoji = obj2.RaidExperienceLevel.DiscordEmoji,
                                                                                                         Description = obj2.RaidExperienceLevel.Description,
                                                                                                         Count = obj2.Count
                                                                                                     })
                                                                                     .ToList(),

                                                              Registrations = obj.RaidRegistrations

                                                                                 // TODO order by points
                                                                                 .OrderBy(obj => obj.RegistrationTimeStamp)
                                                                                 .Select(obj2 => new RaidAppointmentRegistrationData
                                                                                                 {
                                                                                                     UserId = obj2.UserId,
                                                                                                     RaidExperienceLevelId = obj2.User.RaidExperienceLevelId
                                                                                                 })
                                                                                 .ToList()
                                                          })
                                           .FirstOrDefault();

                if (appointment != null)
                {
                    var builder = new DiscordEmbedBuilder();

                    var channel = await _client.GetChannelAsync(appointment.ChannelId)
                                               .ConfigureAwait(false);

                    if (channel != null)
                    {
                        var message = await channel.GetMessageAsync(appointment.MessageId)
                                                   .ConfigureAwait(false);

                        if (message != null)
                        {
                            var slots = GetSlots(dbFactory, appointment);

                            // Assigning the users to the slots
                            var substitutesBench = new List<(ulong UserId, long? ExperienceLevelId)>();

                            foreach (var registration in appointment.Registrations)
                            {
                                var slot = slots.FirstOrDefault(obj => obj.SlotCount > obj.Users.Count
                                                                       && obj.ExperienceLevelIds.Any(obj2 => obj2 == registration.RaidExperienceLevelId));
                                if (slot != null)
                                {
                                    slot.Users.Add(registration.UserId);
                                }
                                else
                                {
                                    substitutesBench.Add((registration.UserId, registration.RaidExperienceLevelId));
                                }
                            }

                            var stringBuilder = new StringBuilder();

                            // Building the message
                            foreach (var slot in slots)
                            {
                                foreach (var userId in slot.Users)
                                {
                                    var discordUser = await _client.GetUserAsync(userId)
                                                                   .ConfigureAwait(false);

                                    stringBuilder.AppendLine($" > {DiscordEmojiService.GetQuestionMarkEmoji(_client)} {discordUser.Mention}");
                                }

                                stringBuilder.Append('\u200B');

                                builder.AddField($"{DiscordEmojiService.GetGuildEmoji(_client, slot.DiscordEmoji)} {slot.Description} ({slot.Users.Count}/{slot.SlotCount})", stringBuilder.ToString());

                                stringBuilder.Clear();
                            }

                            foreach (var (userId, experienceLevelId) in substitutesBench)
                            {
                                var discordUser = await _client.GetUserAsync(userId)
                                                               .ConfigureAwait(false);

                                var experienceLevel = _unorderedLevels.FirstOrDefault(obj => obj.Id == experienceLevelId);

                                stringBuilder.AppendLine($" > {DiscordEmojiService.GetQuestionMarkEmoji(_client)} {discordUser.Mention} {(experienceLevel != null ? DiscordEmojiService.GetGuildEmoji(_client, experienceLevel.DiscordEmoji) : null)}");
                            }

                            stringBuilder.Append('\u200B');

                            builder.AddField(LocalizationGroup.GetText("SubstitutesBench", "Substitutes bench"), stringBuilder.ToString());

                            builder.WithTitle($"{appointment.Title} - {appointment.TimeStamp:g}");
                            builder.WithDescription(appointment.Description);
                            builder.WithThumbnail(appointment.Thumbnail);
                            builder.WithColor(DiscordColor.Green);
                            builder.WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/836238701046398987/d7d1b509a23aa9789885127da9107fe0.png?size=256");
                            builder.WithTimestamp(DateTime.Now);

                            await message.ModifyAsync(null, builder.Build())
                                         .ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the usable slots of the appointment
        /// </summary>
        /// <param name="dbFactory">Repository factory</param>
        /// <param name="appointment">Appointment data</param>
        /// <returns>Slots</returns>
        private List<RaidAppointmentSlotData> GetSlots(RepositoryFactory dbFactory, RaidAppointmentMessageData appointment)
        {
            var slotCountFactor = appointment.Registrations.Count / appointment.ExperienceLevels.Sum(obj => (double)obj.Count) > 1.4 ? 2 : 1;

            _unorderedLevels ??= dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                          .GetQuery()
                                          .Where(obj => obj.IsDeleted == false)
                                          .Select(obj => new RaidExperienceLevelData
                                                         {
                                                             Id = obj.Id,
                                                             SuperiorExperienceLevelId = obj.SuperiorExperienceLevelId,
                                                             Description = obj.Description,
                                                             DiscordEmoji = obj.DiscordEmoji,
                                                         })
                                          .ToList();

            var slots = new List<RaidAppointmentSlotData>();
            var currentLevel = _unorderedLevels.FirstOrDefault(obj => obj.SuperiorExperienceLevelId == null);
            var experienceLevelIds = new List<long>();

            while (currentLevel != null)
            {
                experienceLevelIds.Add(currentLevel.Id);

                var currentSlot = new RaidAppointmentSlotData
                                  {
                                      Description = currentLevel.Description,
                                      DiscordEmoji = currentLevel.DiscordEmoji,
                                      Users = new List<ulong>(),
                                      ExperienceLevelIds = experienceLevelIds.ToList()
                                  };

                slots.Add(currentSlot);

                var experienceLevel = appointment.ExperienceLevels.FirstOrDefault(obj => obj.Id == currentLevel.Id);
                if (experienceLevel != null)
                {
                    currentSlot.SlotCount = experienceLevel.Count * slotCountFactor;
                }

                currentLevel = _unorderedLevels.FirstOrDefault(obj => obj.SuperiorExperienceLevelId == currentLevel.Id);
            }

            slots.RemoveAll(obj => obj.SlotCount == 0);

            return slots;
        }

        #endregion // Methods
    }
}

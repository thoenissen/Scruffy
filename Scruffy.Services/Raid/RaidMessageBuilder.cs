using DSharpPlus;
using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Raid;

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
            var currentRaidPoints = dbFactory.GetRepository<RaidCurrentUserPointsRepository>()
                                             .GetQuery()
                                             .Select(obj => obj);

            var appointment = dbFactory.GetRepository<RaidAppointmentRepository>()
                                       .GetQuery()
                                       .Where(obj => obj.ConfigurationId == configurationId
                                                  && obj.IsCommitted == false)
                                       .OrderByDescending(obj => obj.TimeStamp)
                                       .Select(obj => new
                                                      {
                                                          obj.TimeStamp,
                                                          ChannelId = obj.RaidDayConfiguration.DiscordChannelId,
                                                          MessageId = obj.RaidDayConfiguration.DiscordMessageId,
                                                          obj.RaidDayTemplate.Thumbnail,
                                                          obj.RaidDayTemplate.Title,
                                                          obj.RaidDayTemplate.Description,

                                                          ExperienceLevels  = obj.RaidDayTemplate
                                                                                 .RaidExperienceAssignments
                                                                                 .Select(obj2 => new
                                                                                                 {
                                                                                                     RaidExperienceLevelId  = obj2.RaidExperienceLevel.Id,
                                                                                                     obj2.RaidExperienceLevel.DiscordEmoji,
                                                                                                     obj2.RaidExperienceLevel.Description,
                                                                                                     obj2.RaidExperienceLevel.Rank,
                                                                                                     obj2.Count
                                                                                                 })
                                                                                 .ToList(),

                                                          Registrations = obj.RaidRegistrations
                                                                             .Select(obj3 => new
                                                                                             {
                                                                                                 UserId = obj3.User
                                                                                                              .DiscordAccounts
                                                                                                              .Select(obj4 => obj4.Id)
                                                                                                              .FirstOrDefault(),
                                                                                                 Points = currentRaidPoints.Where(obj4 => obj4.UserId == obj3.UserId)
                                                                                                                           .Select(obj4 => obj4.Points).FirstOrDefault(),
                                                                                                 obj3.LineupExperienceLevelId,
                                                                                                 ExperienceLevelId = (int?)obj3.User.RaidExperienceLevel.Id,
                                                                                                 ExperienceLevelDiscordEmoji = (ulong?)obj3.User.RaidExperienceLevel.DiscordEmoji,
                                                                                                 Roles = obj3.RaidRegistrationRoleAssignments
                                                                                                             .Select(obj4 => new
                                                                                                                             {
                                                                                                                                 MainRoleEmoji = obj4.MainRaidRole.DiscordEmojiId,
                                                                                                                                 SubRoleEmoji = (ulong?)obj4.SubRaidRole.DiscordEmojiId
                                                                                                                             })
                                                                                                             .ToList()
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
                        var fieldBuilder = new StringBuilder();

                        var slotCountFactor = appointment.Registrations.Count / appointment.ExperienceLevels.Sum(obj => (double)obj.Count) >= 1.4
                                                  ? 2
                                                  : 1;

                        // Building the message
                        foreach (var slot in appointment.ExperienceLevels
                                                        .OrderBy(obj => obj.Rank))
                        {
                            var fieldCounter = 1;

                            var registrations = appointment.Registrations
                                                           .Where(obj => obj.LineupExperienceLevelId == slot.RaidExperienceLevelId)
                                                           .OrderByDescending(obj => obj.Points)
                                                           .ToList();

                            foreach (var registration in registrations)
                            {
                                var discordUser = await _client.GetUserAsync(registration.UserId)
                                                               .ConfigureAwait(false);

                                var lineBuilder = new StringBuilder();
                                lineBuilder.Append(" > ");

                                if (registration.Roles?.Count > 0)
                                {
                                    var first = true;

                                    foreach (var role in registration.Roles)
                                    {
                                        if (first == false)
                                        {
                                            lineBuilder.Append(", ");
                                        }
                                        else
                                        {
                                            first = false;
                                        }

                                        lineBuilder.Append(DiscordEmojiService.GetGuildEmoji(_client, role.MainRoleEmoji));

                                        if (role.SubRoleEmoji != null)
                                        {
                                            lineBuilder.Append(DiscordEmojiService.GetGuildEmoji(_client, role.SubRoleEmoji.Value));
                                        }
                                    }
                                }
                                else
                                {
                                    lineBuilder.Append(DiscordEmojiService.GetQuestionMarkEmoji(_client));
                                }

                                lineBuilder.Append($" {discordUser.Mention}");

                                if (registration.LineupExperienceLevelId != registration.ExperienceLevelId
                                 && registration.ExperienceLevelDiscordEmoji != null)
                                {
                                    lineBuilder.Append(' ');
                                    lineBuilder.Append(DiscordEmojiService.GetGuildEmoji(_client, registration.ExperienceLevelDiscordEmoji.Value));
                                }

                                lineBuilder.Append('\n');

                                if (lineBuilder.Length + fieldBuilder.Length > 1024)
                                {
                                    builder.AddField($"{DiscordEmojiService.GetGuildEmoji(_client, slot.DiscordEmoji)} {slot.Description} ({registrations.Count}/{slot.Count * slotCountFactor}) #{fieldCounter}", fieldBuilder.ToString());
                                    fieldBuilder = new StringBuilder();
                                    fieldCounter++;
                                }

                                fieldBuilder.Append(lineBuilder);
                            }

                            fieldBuilder.Append('\u200B');

                            var fieldName = $"{DiscordEmojiService.GetGuildEmoji(_client, slot.DiscordEmoji)} {slot.Description} ({registrations.Count}/{slot.Count * slotCountFactor})";
                            if (fieldCounter != 1)
                            {
                                fieldName = $"{fieldName} #{fieldCounter}";
                            }

                            builder.AddField(fieldName, fieldBuilder.ToString());

                            fieldBuilder.Clear();
                        }

                        foreach (var entry in appointment.Registrations
                                                         .Where(obj => obj.LineupExperienceLevelId == null)
                                                         .OrderByDescending(obj => obj.Points))
                        {
                            var discordUser = await _client.GetUserAsync(entry.UserId)
                                                           .ConfigureAwait(false);

                            fieldBuilder.Append(" > ");

                            if (entry.Roles?.Count > 0)
                            {
                                var first = true;

                                foreach (var role in entry.Roles)
                                {
                                    if (first == false)
                                    {
                                        fieldBuilder.Append(", ");
                                    }
                                    else
                                    {
                                        first = false;
                                    }

                                    fieldBuilder.Append(DiscordEmojiService.GetGuildEmoji(_client, role.MainRoleEmoji));

                                    if (role.SubRoleEmoji != null)
                                    {
                                        fieldBuilder.Append(DiscordEmojiService.GetGuildEmoji(_client, role.SubRoleEmoji.Value));
                                    }
                                }
                            }
                            else
                            {
                                fieldBuilder.Append(DiscordEmojiService.GetQuestionMarkEmoji(_client));
                            }

                            fieldBuilder.AppendLine($" {discordUser.Mention} {(entry.ExperienceLevelDiscordEmoji != null ? DiscordEmojiService.GetGuildEmoji(_client, entry.ExperienceLevelDiscordEmoji.Value) : null)}");
                        }

                        fieldBuilder.Append('\u200B');

                        builder.AddField(LocalizationGroup.GetText("SubstitutesBench", "Substitutes bench"), fieldBuilder.ToString());

                        builder.WithTitle($"{appointment.Title} - {appointment.TimeStamp.ToString("g", LocalizationGroup.CultureInfo)}");
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

    #endregion // Methods
}
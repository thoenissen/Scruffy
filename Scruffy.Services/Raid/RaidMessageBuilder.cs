using Discord;
using Discord.WebSocket;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid;

/// <summary>
/// Building the lfg message
/// </summary>
public class RaidMessageBuilder : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Raid roles
    /// </summary>
    private static List<SelectMenuOptionBuilder> _entries;

    /// <summary>
    /// Discord client
    /// </summary>
    private readonly DiscordSocketClient _client;

    /// <summary>
    /// Raid roles service
    /// </summary>
    private readonly RaidRolesService _raidRolesService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <param name="localizationService">Localization service</param>
    /// <param name="raidRolesService">Raid roles service</param>
    public RaidMessageBuilder(DiscordSocketClient client, LocalizationService localizationService, RaidRolesService raidRolesService)
        : base(localizationService)
    {
        _client = client;
        _raidRolesService = raidRolesService;
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
                                                          obj.Deadline,
                                                          ChannelId = obj.RaidDayConfiguration.DiscordChannelId,
                                                          MessageId = obj.RaidDayConfiguration.DiscordMessageId,
                                                          obj.RaidDayConfiguration.AliasName,
                                                          obj.RaidDayTemplate.Thumbnail,
                                                          obj.RaidDayTemplate.Title,
                                                          obj.RaidDayTemplate.Description,
                                                          obj.GroupCount,

                                                          ExperienceLevels = obj.RaidDayTemplate
                                                                                .RaidExperienceAssignments
                                                                                .Select(obj2 => new
                                                                                                {
                                                                                                    RaidExperienceLevelId = obj2.RaidExperienceLevel.Id,
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
                                                                                                           .Select(obj4 => obj4.Points)
                                                                                                           .FirstOrDefault(),
                                                                                 obj3.LineupExperienceLevelId,
                                                                                 ExperienceLevelId = (int?)obj3.User.RaidExperienceLevel.Id,
                                                                                 ExperienceLevelDiscordEmote = (ulong?)obj3.User.RaidExperienceLevel.DiscordEmoji,
                                                                                 Roles = obj3.RaidRegistrationRoleAssignments
                                                                                             .Select(obj4 => obj4.Role)
                                                                                             .ToList()
                                                                             })
                                                                             .ToList()
                                                      })
                                       .FirstOrDefault();

            if (appointment != null)
            {
                var builder = new EmbedBuilder();

                var channel = await _client.GetChannelAsync(appointment.ChannelId)
                                           .ConfigureAwait(false);

                if (channel is IMessageChannel messageChannel)
                {
                    var message = await messageChannel.GetMessageAsync(appointment.MessageId)
                                                      .ConfigureAwait(false);

                    if (message is IUserMessage userMessage)
                    {
                        var fieldBuilder = new StringBuilder();
                        var areSlotsAvailable = false;
                        int fieldCounter;

                        // Building the message
                        foreach (var slot in appointment.ExperienceLevels
                                                        .OrderBy(obj => obj.Rank))
                        {
                            fieldCounter = 1;

                            var registrations = appointment.Registrations
                                                           .Where(obj => obj.LineupExperienceLevelId == slot.RaidExperienceLevelId)
                                                           .OrderByDescending(obj => obj.Points)
                                                           .ToList();
                            if (registrations.Count < slot.Count* appointment.GroupCount)
                            {
                                areSlotsAvailable = true;
                            }

                            foreach (var registration in registrations)
                            {
                                var discordUser = await _client.GetUserAsync(registration.UserId)
                                                               .ConfigureAwait(false);

                                var lineBuilder = new StringBuilder();
                                lineBuilder.Append(" > ");

                                if (registration.Roles.Count > 0)
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

                                        lineBuilder.Append(_raidRolesService.GetDescriptionAsEmoji(role));
                                    }
                                }
                                else
                                {
                                    lineBuilder.Append(DiscordEmoteService.GetQuestionMarkEmote(_client));
                                }

                                lineBuilder.Append($" {discordUser.Mention}");

                                if (registration.LineupExperienceLevelId != registration.ExperienceLevelId
                                 && registration.ExperienceLevelDiscordEmote != null)
                                {
                                    lineBuilder.Append(' ');
                                    lineBuilder.Append(DiscordEmoteService.GetGuildEmote(_client, registration.ExperienceLevelDiscordEmote.Value));
                                }

                                lineBuilder.Append('\n');

                                if (lineBuilder.Length + fieldBuilder.Length > 1024)
                                {
                                    builder.AddField($"{DiscordEmoteService.GetGuildEmote(_client, slot.DiscordEmoji)} {slot.Description} ({registrations.Count}/{slot.Count * appointment.GroupCount}) #{fieldCounter}", fieldBuilder.ToString());
                                    fieldBuilder = new StringBuilder();
                                    fieldCounter++;
                                }

                                fieldBuilder.Append(lineBuilder);
                            }

                            fieldBuilder.Append('\u200B');

                            var fieldName = $"{DiscordEmoteService.GetGuildEmote(_client, slot.DiscordEmoji)} {slot.Description} ({registrations.Count}/{slot.Count * appointment.GroupCount})";
                            if (fieldCounter > 1)
                            {
                                fieldName = $"{fieldName} #{fieldCounter}";
                            }

                            builder.AddField(fieldName, fieldBuilder.ToString());

                            fieldBuilder.Clear();
                        }

                        fieldCounter = 1;

                        foreach (var entry in appointment.Registrations
                                                         .Where(obj => obj.LineupExperienceLevelId == null)
                                                         .OrderByDescending(obj => obj.Points))
                        {
                            var discordUser = await _client.GetUserAsync(entry.UserId)
                                                           .ConfigureAwait(false);

                            var lineBuilder = new StringBuilder();
                            lineBuilder.Append(" > ");

                            if (entry.Roles?.Count > 0)
                            {
                                var first = true;

                                foreach (var role in entry.Roles)
                                {
                                    if (first == false)
                                    {
                                        lineBuilder.Append(", ");
                                    }
                                    else
                                    {
                                        first = false;
                                    }

                                    lineBuilder.Append(_raidRolesService.GetDescriptionAsEmoji(role));
                                }
                            }
                            else
                            {
                                lineBuilder.Append(DiscordEmoteService.GetQuestionMarkEmote(_client));
                            }

                            lineBuilder.AppendLine($" {discordUser.Mention} {(entry.ExperienceLevelDiscordEmote != null ? DiscordEmoteService.GetGuildEmote(_client, entry.ExperienceLevelDiscordEmote.Value) : null)}");

                            if (lineBuilder.Length + fieldBuilder.Length > 1024)
                            {
                                builder.AddField($"{LocalizationGroup.GetText("SubstitutesBench", "Substitutes bench")} #{fieldCounter}", fieldBuilder.ToString());
                                fieldBuilder = new StringBuilder();
                                fieldCounter++;
                            }

                            fieldBuilder.Append(lineBuilder);
                        }

                        fieldBuilder.Append('\u200B');

                        var substitutesBenchFieldName = LocalizationGroup.GetText("SubstitutesBench", "Substitutes bench");
                        if (fieldCounter > 1)
                        {
                            substitutesBenchFieldName = $"{substitutesBenchFieldName} #{fieldCounter}";
                        }

                        builder.AddField(substitutesBenchFieldName, fieldBuilder.ToString());

                        builder.WithTitle($"{appointment.Title} - {appointment.TimeStamp.ToString("g", LocalizationGroup.CultureInfo)}");
                        builder.WithDescription(appointment.Description);
                        builder.WithThumbnailUrl(appointment.Thumbnail);
                        builder.WithColor(Color.Green);
                        builder.WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64");
                        builder.WithTimestamp(DateTime.Now);

                        var componentsBuilder = new ComponentBuilder();

                        componentsBuilder.WithButton(LocalizationGroup.GetText("Join", "Join"),
                                                     InteractivityService.GetPermanentCustomId("raid",
                                                                                                 "join",
                                                                                                 appointment.AliasName),
                                                     ButtonStyle.Secondary,
                                                     DiscordEmoteService.GetCheckEmote(_client),
                                                     null,
                                                     (appointment.Deadline <= DateTime.Now && areSlotsAvailable == false)
                                                  || appointment.TimeStamp <= DateTime.Now);

                        if (appointment.Deadline > DateTime.Now
                         && appointment.TimeStamp > DateTime.Now)
                        {
                            componentsBuilder.WithSelectMenu($"raid;select_roles;{configurationId}",
                                                             GetRaidRoleOptions(),
                                                             LocalizationGroup.GetText("ChooseRoleDescription", "Chose your roles..."),
                                                             1,
                                                             9);
                        }

                        componentsBuilder.WithButton(LocalizationGroup.GetText("Leave", "Leave"),
                                                     InteractivityService.GetPermanentCustomId("raid",
                                                                                                 "leave",
                                                                                                 appointment.AliasName),
                                                     ButtonStyle.Secondary,
                                                     DiscordEmoteService.GetCrossEmote(_client),
                                                     null,
                                                     appointment.TimeStamp <= DateTime.Now);

                        await userMessage.ModifyAsync(obj =>
                                                      {
                                                          obj.Content = "\u200b";
                                                          obj.Embed = builder.Build();
                                                          obj.Components = componentsBuilder.Build();
                                                      })
                                         .ConfigureAwait(false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Raid role selection options
    /// </summary>
    /// <returns>List of options</returns>
    private List<SelectMenuOptionBuilder> GetRaidRoleOptions()
    {
        if (_entries == null)
        {
            _entries = new List<SelectMenuOptionBuilder>();

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var roles = dbFactory.GetRepository<RaidRoleRepository>()
                                     .GetQuery()
                                     .OrderBy(obj => obj.Id)
                                     .ToList();

                foreach (var role in roles)
                {
                    _entries.Add(new SelectMenuOptionBuilder().WithLabel(_raidRolesService.GetDescriptionAsText(role))
                                                              .WithEmote(DiscordEmoteService.GetGuildEmote(_client, role.DiscordEmojiId))
                                                              .WithValue(role.Id.ToString()));
                }
            }
        }

        return _entries;
    }

    #endregion // Methods
}
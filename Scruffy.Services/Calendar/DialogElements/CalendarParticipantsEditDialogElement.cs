using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Data.Services.Calendar;
using Scruffy.Services.Core.Extensions;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar.DialogElements;

/// <summary>
/// Editing the participants of an appointment
/// </summary>
public class CalendarParticipantsEditDialogElement : DialogEmbedSelectMenuElementBase<bool>
{
    #region Fields

    /// <summary>
    /// Data
    /// </summary>
    private AppointmentParticipantsContainer _data;

    /// <summary>
    /// Reactions
    /// </summary>
    private List<SelectMenuEntryData<bool>> _entries;

    /// <summary>
    /// User management service
    /// </summary>
    private UserManagementService _userManagementService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="userManagementService">User management</param>
    /// <param name="data">Data</param>
    public CalendarParticipantsEditDialogElement(LocalizationService localizationService, UserManagementService userManagementService, AppointmentParticipantsContainer data)
        : base(localizationService)
    {
        _data = data;
        _userManagementService = userManagementService;
    }

    #endregion // Constructor

    #region DialogEmbedReactionElementBase<bool>

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override async Task<EmbedBuilder> GetMessage()
    {
        var builder = new EmbedBuilder().WithTitle(LocalizationGroup.GetText("CommitTitle", "Participants"))
                                        .WithColor(Color.Green)
                                        .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                        .WithTimestamp(DateTime.Now);

        var message = new StringBuilder();

        if (_data.Participants == null)
        {
            _data.Participants = new List<CalendarAppointmentParticipantData>();

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                foreach (var entry in dbFactory.GetRepository<CalendarAppointmentParticipantRepository>()
                                               .GetQuery()
                                               .Where(obj => obj.AppointmentId == _data.AppointmentId)
                                               .Select(obj => new
                                                              {
                                                                  UserId = obj.User
                                                                              .DiscordAccounts
                                                                              .Select(obj2 => obj2.Id)
                                                                              .FirstOrDefault(),
                                                                  obj.IsLeader
                                                              }))
                {
                    _data.Participants.Add(new CalendarAppointmentParticipantData
                                           {
                                               Member = await CommandContext.Guild
                                                                            .GetUserAsync(entry.UserId)
                                                                            .ConfigureAwait(false),
                                               IsLeader = entry.IsLeader
                                           });
                }

                if (_data.Participants.Count == 0)
                {
                    var userId = dbFactory.GetRepository<CalendarAppointmentRepository>()
                                          .GetQuery()
                                          .Where(obj => obj.Id == _data.AppointmentId)
                                          .Select(obj => obj.Leader
                                                            .DiscordAccounts
                                                            .Select(obj2 => (ulong?)obj2.Id)
                                                            .FirstOrDefault())
                                          .FirstOrDefault();

                    if (userId != null)
                    {
                        _data.Participants.Add(new CalendarAppointmentParticipantData
                                               {
                                                   Member = await CommandContext.Guild
                                                                                .GetUserAsync(userId.Value)
                                                                                .ConfigureAwait(false),
                                                   IsLeader = true
                                               });
                    }
                }
            }
        }

        if (_data.Participants.Count > 0)
        {
            builder.WithDescription(LocalizationGroup.GetFormattedText("CommitCountText", "The following {0} participants are recorded:", _data.Participants.Count));
        }
        else
        {
            builder.WithDescription(LocalizationGroup.GetText("CommitText", "The following participants are recorded:"));
        }

        var fieldCounter = 1;

        foreach (var entry in _data.Participants
                                   .OrderBy(obj => obj.Member.TryGetDisplayName()))
        {
            var line = $"> {entry.Member.Mention}";

            if (entry.IsLeader)
            {
                line += ' ';
                line += DiscordEmoteService.GetStarEmote(CommandContext.Client);
            }

            line += '\n';

            if (line.Length + message.Length > 1000)
            {
                builder.AddField(LocalizationGroup.GetText("Participants", "Participants") + " #" + fieldCounter, message.ToString());

                message = new StringBuilder();
                fieldCounter++;
            }

            message.Append(line);
        }

        if (message.Length == 0)
        {
            message.AppendLine("\u200b");
        }

        var fieldTitle = LocalizationGroup.GetText("Participants", "Participants");
        if (fieldCounter > 1)
        {
            fieldTitle += " #" + fieldCounter;
        }

        builder.AddField(fieldTitle, message.ToString());

        return builder;
    }

    /// <summary>
    /// Default case if none of the given reactions is used
    /// </summary>
    /// <returns>Result</returns>
    protected override bool DefaultFunc() => false;

    /// <summary>
    /// Returns the select menu entries which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public override IReadOnlyList<SelectMenuEntryData<bool>> GetEntries()
    {
        return _entries ??= new List<SelectMenuEntryData<bool>>
                            {
                                new()
                                {
                                    Emote = DiscordEmoteService.GetAddEmote(CommandContext.Client),
                                    CommandText = LocalizationGroup.GetText("AddUserCommand", "Add user"),
                                    Response = async () =>
                                               {
                                                   var members = await RunSubElement<CalendarAddParticipantsDialogElement, List<IGuildUser>>().ConfigureAwait(false);
                                                   if (members != null)
                                                   {
                                                       foreach (var member in members)
                                                       {
                                                           if (_data.Participants.Any(obj => obj.Member.Id == member.Id) == false)
                                                           {
                                                               _data.Participants.Add(new CalendarAppointmentParticipantData
                                                                                      {
                                                                                          Member = member,
                                                                                          IsLeader = false
                                                                                      });
                                                           }
                                                       }
                                                   }

                                                   return true;
                                               }
                                },
                                new()
                                {
                                    Emote = DiscordEmoteService.GetAdd2Emote(CommandContext.Client),
                                    CommandText = LocalizationGroup.GetText("AddVoiceChannelCommand", "Add channel"),
                                    Response = async () =>
                                               {
                                                   var members = await RunSubElement<CalendarAddVoiceChannelDialogElement, List<IGuildUser>>().ConfigureAwait(false);
                                                   if (members != null)
                                                   {
                                                       foreach (var member in members)
                                                       {
                                                           if (_data.Participants.Any(obj => obj.Member.Id == member.Id) == false)
                                                           {
                                                               _data.Participants.Add(new CalendarAppointmentParticipantData
                                                                                      {
                                                                                          Member = member,
                                                                                          IsLeader = false
                                                                                      });
                                                           }
                                                       }
                                                   }

                                                   return true;
                                               }
                                },
                                new()
                                {
                                    Emote = DiscordEmoteService.GetTrashCanEmote(CommandContext.Client),
                                    CommandText = LocalizationGroup.GetText("RemoveUserCommand", "Remove user"),
                                    Response = async () =>
                                               {
                                                   var members = await RunSubElement<CalendarRemoveParticipantsDialogElement, List<IGuildUser>>().ConfigureAwait(false);
                                                   if (members != null)
                                                   {
                                                       foreach (var member in members)
                                                       {
                                                           var entry = _data.Participants.FirstOrDefault(obj => obj.Member.Id == member.Id);
                                                           if (entry != null)
                                                           {
                                                               _data.Participants.Remove(entry);
                                                           }
                                                       }
                                                   }

                                                   return true;
                                               }
                                },
                                new()
                                {
                                    Emote = DiscordEmoteService.GetStarEmote(CommandContext.Client),
                                    CommandText = LocalizationGroup.GetText("SetLeaderCommand", "Leader"),
                                    Response = async () =>
                                               {
                                                   var members = await RunSubElement<CalendarRemoveParticipantsDialogElement, List<IGuildUser>>().ConfigureAwait(false);
                                                   if (members != null)
                                                   {
                                                       foreach (var member in members)
                                                       {
                                                           var entry = _data.Participants.FirstOrDefault(obj => obj.Member.Id == member.Id);
                                                           if (entry != null)
                                                           {
                                                               entry.IsLeader = true;
                                                           }
                                                           else
                                                           {
                                                               _data.Participants.Add(new CalendarAppointmentParticipantData
                                                                                      {
                                                                                          Member = member,
                                                                                          IsLeader = true
                                                                                      });
                                                           }
                                                       }
                                                   }

                                                   return true;
                                               }
                                },
                                new()
                                {
                                    Emote = DiscordEmoteService.GetCheckEmote(CommandContext.Client),
                                    CommandText = LocalizationGroup.GetText("CommitCommand", "Commit"),
                                    Response = async () =>
                                               {
                                                   using (var dbFactory = RepositoryFactory.CreateInstance())
                                                   {
                                                       dbFactory.GetRepository<CalendarAppointmentParticipantRepository>()
                                                                .RemoveRange(obj => obj.AppointmentId == _data.AppointmentId);

                                                       foreach (var entry in _data.Participants)
                                                       {
                                                           await _userManagementService.CheckDiscordAccountAsync(entry.Member)
                                                                                       .ConfigureAwait(false);

                                                           var user = await _userManagementService.GetUserByDiscordAccountId(entry.Member)
                                                                                                  .ConfigureAwait(false);

                                                           if (dbFactory.GetRepository<CalendarAppointmentParticipantRepository>()
                                                                        .Add(new Data.Entity.Tables.Calendar.CalendarAppointmentParticipantEntity
                                                                             {
                                                                                 AppointmentId = _data.AppointmentId,
                                                                                 UserId = user.Id,
                                                                                 IsLeader = entry.IsLeader
                                                                             }) == false)
                                                           {
                                                               throw dbFactory.LastError;
                                                           }
                                                       }
                                                   }

                                                   await CommandContext.Channel
                                                                       .SendMessageAsync(LocalizationGroup.GetText("CommitSuccess", "The participants list has been refreshed."))
                                                                       .ConfigureAwait(false);

                                                   return false;
                                               }
                                },
                                new()
                                {
                                    Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                                    CommandText = LocalizationGroup.GetText("CancelCommand", "Cancel"),
                                    Response = () => Task.FromResult(false)
                                },
                            };
    }

    #endregion // DialogEmbedReactionElementBase<bool>
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Data.Services.Calendar;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Extensions;
using Scruffy.Services.CoreData;

namespace Scruffy.Services.Calendar.DialogElements
{
    /// <summary>
    /// Editing the participants of an appointment
    /// </summary>
    public class CalendarParticipantsEditDialogElement : DialogEmbedReactionElementBase<bool>
    {
        #region Fields

        /// <summary>
        /// Data
        /// </summary>
        private AppointmentParticipantsContainer _data;

        /// <summary>
        /// Reactions
        /// </summary>
        private List<ReactionData<bool>> _reactions;

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
        /// Editing the embedded message
        /// </summary>
        /// <param name="builder">Builder</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task EditMessage(DiscordEmbedBuilder builder)
        {
            builder.WithTitle(LocalizationGroup.GetText("CommitTitle", "Participants"));
            builder.WithDescription(LocalizationGroup.GetText("CommitText", "The following participants are recorded:"));
            builder.WithColor(DiscordColor.Green);
            builder.WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/ef1f3e1f3f40100fb3750f8d7d25c657.png?size=64");
            builder.WithTimestamp(DateTime.Now);

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
                                                                      obj.UserId,
                                                                      obj.IsLeader
                                                                  }))
                    {
                        _data.Participants.Add(new CalendarAppointmentParticipantData
                                               {
                                                   Member = await CommandContext.Guild
                                                                                .GetMemberAsync(entry.UserId)
                                                                                .ConfigureAwait(false),
                                                   IsLeader = entry.IsLeader
                                               });
                    }
                }
            }

            foreach (var entry in _data.Participants
                                      .OrderBy(obj => obj.Member.TryGetDisplayName()))
            {
                message.Append($"> {entry.Member.Mention}");

                if (entry.IsLeader)
                {
                    message.Append(' ');
                    message.Append(DiscordEmojiService.GetStarEmoji(CommandContext.Client));
                }

                message.Append('\n');
            }

            message.AppendLine("\u200b");

            builder.AddField($"{LocalizationGroup.GetText("Participants", "Participants")} ({_data.Participants.Count})", message.ToString());
        }

        /// <summary>
        /// Returns the title of the commands
        /// </summary>
        /// <returns>Commands</returns>
        protected override string GetCommandTitle() => LocalizationGroup.GetText("Commands", "Commands");

        /// <summary>
        /// Default case if none of the given reactions is used
        /// </summary>
        /// <returns>Result</returns>
        protected override bool DefaultFunc() => false;

        /// <summary>
        /// Returns the reactions which should be added to the message
        /// </summary>
        /// <returns>Reactions</returns>
        public override IReadOnlyList<ReactionData<bool>> GetReactions()
        {
            return _reactions ??= new List<ReactionData<bool>>
                                  {
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetAddEmoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("AddUserCommand", "{0} Add user", DiscordEmojiService.GetAddEmoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var members = await RunSubElement<CalendarAddParticipantsDialogElement, List<DiscordMember>>().ConfigureAwait(false);
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
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetAdd2Emoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("AddVoiceChannelCommand", "{0} Add channel", DiscordEmojiService.GetAdd2Emoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var members = await RunSubElement<CalendarAddVoiceChannelDialogElement, List<DiscordMember>>().ConfigureAwait(false);
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
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetTrashCanEmoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("RemoveUserCommand", "{0} Remove user", DiscordEmojiService.GetTrashCanEmoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var members = await RunSubElement<CalendarRemoveParticipantsDialogElement, List<DiscordMember>>().ConfigureAwait(false);
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
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetStarEmoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("SetLeaderCommand", "{0} Leader", DiscordEmojiService.GetStarEmoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var members = await RunSubElement<CalendarRemoveParticipantsDialogElement, List<DiscordMember>>().ConfigureAwait(false);
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
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetCheckEmoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("CommitCommand", "{0} Commit", DiscordEmojiService.GetCheckEmoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     using (var dbFactory = RepositoryFactory.CreateInstance())
                                                     {
                                                         dbFactory.GetRepository<CalendarAppointmentParticipantRepository>()
                                                                  .RemoveRange(obj => obj.AppointmentId == _data.AppointmentId);

                                                         foreach (var entry in _data.Participants)
                                                         {
                                                             await _userManagementService.CheckUserAsync(entry.Member.Id)
                                                                                         .ConfigureAwait(false);

                                                             if (dbFactory.GetRepository<CalendarAppointmentParticipantRepository>()
                                                                      .Add(new Data.Entity.Tables.Calendar.CalendarAppointmentParticipantEntity
                                                                           {
                                                                               AppointmentId = _data.AppointmentId,
                                                                               UserId = entry.Member.Id,
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
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetCrossEmoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("CancelCommand", "{0} Cancel", DiscordEmojiService.GetCrossEmoji(CommandContext.Client)),
                                          Func = () => Task.FromResult(false)
                                      },
                                  };
        }

        #endregion // DialogEmbedReactionElementBase<bool>
    }
}
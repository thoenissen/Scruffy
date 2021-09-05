using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Reminder;
using Scruffy.Data.Entity.Tables.Reminder;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.CoreData;

namespace Scruffy.Commands
{
    /// <summary>
    /// Reminder module
    /// </summary>
    [ModuleLifespan(ModuleLifespan.Transient)]
    [Group("reminder")]
    [Aliases("re")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public class ReminderCreationCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public ReminderCreationCommandModule(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// User management service
        /// </summary>
        public UserManagementService UserManagementService { get; set; }

        /// <summary>
        /// Scheduling jobs
        /// </summary>
        public JobScheduler JobScheduler { get; set; }

        #endregion // Properties

        #region Command methods

        /// <summary>
        /// Creation of a one time reminder
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="timeSpan">Timespan until the reminder should be executed.</param>
        /// <param name="message">Optional message of the reminder</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("in")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
        public Task RemindMeIn(CommandContext commandContext, string timeSpan, [RemainingText] string message = null)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   var checkUser = UserManagementService.CheckUserAsync(commandContext.User.Id);

                                   var timeSpanValidation = new Regex(@"\d+(h|m|s)");
                                   if (timeSpanValidation.IsMatch(timeSpan))
                                   {
                                       var amount = Convert.ToUInt64(timeSpan[..^1], CultureInfo.InvariantCulture);

                                       using (var dbFactory = RepositoryFactory.CreateInstance())
                                       {
                                           await checkUser.ConfigureAwait(false);

                                           var reminderEntity = new OneTimeReminderEntity
                                                                {
                                                                    UserId = commandContext.User.Id,
                                                                    ChannelId = commandContext.Channel.Id,
                                                                    TimeStamp = timeSpan[^1..] switch
                                                                    {
                                                                        "h" => DateTime.Now.AddHours(amount),
                                                                        "m" => DateTime.Now.AddMinutes(amount),
                                                                        "s" => DateTime.Now.AddSeconds(amount),
                                                                        _ => throw new InvalidOperationException()
                                                                    },
                                                                    Message = message
                                                                };

                                           if (dbFactory.GetRepository<OneTimeReminderRepository>()
                                                        .Add(reminderEntity))
                                           {
                                               JobScheduler.AddOneTimeReminder(reminderEntity.TimeStamp, reminderEntity.Id);

                                               await commandContext.Channel
                                                                   .SendMessageAsync(LocalizationGroup.GetFormattedText("ReminderCreated",
                                                                                                                        "The reminder has been set for {0} {1}.",
                                                                                                                        reminderEntity.TimeStamp
                                                                                                                                      .ToString(LocalizationGroup.CultureInfo.DateTimeFormat.ShortDatePattern,
                                                                                                                                                LocalizationGroup.CultureInfo.DateTimeFormat),
                                                                                                                        reminderEntity.TimeStamp
                                                                                                                                      .ToString(LocalizationGroup.CultureInfo.DateTimeFormat.ShortTimePattern,
                                                                                                                                                LocalizationGroup.CultureInfo.DateTimeFormat)))
                                                                   .ConfigureAwait(false);

                                               if (commandContext.Channel.IsPrivate == false)
                                               {
                                                   await commandContext.Channel
                                                                       .DeleteMessageAsync(commandContext.Message, LocalizationGroup.GetText("CommandProgressed", "Command progressed."))
                                                                       .ConfigureAwait(false);
                                               }
                                           }
                                       }
                                   }
                                   else
                                   {
                                       await commandContextContainer.ShowHelp("reminder in")
                                                                    .ConfigureAwait(false);
                                   }
                               });
        }

        /// <summary>
        /// Creation of a one time reminder
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="time">Time</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("at")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
        public Task RemindMeAt(CommandContext commandContext, string time)
        {
            return RemindMeAt(commandContext, time, null, null);
        }

        /// <summary>
        /// Creation of a one time reminder
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="time">Time</param>
        /// <param name="message">Optional message of the reminder</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("at")]
        public Task RemindMeAt(CommandContext commandContext, string time, string message)
        {
            return RemindMeAt(commandContext, time, message, null);
        }

        /// <summary>
        /// Creation of a one time reminder
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="date">Date</param>
        /// <param name="time">Time</param>
        /// <param name="message">Optional message of the reminder</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("at")]
        public Task RemindMeAt(CommandContext commandContext, string date, string time, [RemainingText] string message)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   var checkUser = UserManagementService.CheckUserAsync(commandContext.User.Id);

                                   DateTime? timeStamp = null;
                                   string reminderMessage = null;

                                   if (new Regex(@"\d\d\d\d-\d\d-\d\d").IsMatch(date)
                                    && DateTime.TryParseExact(date, "yyyy-MM-dd", null, DateTimeStyles.None, out var parsedDate)
                                    && string.IsNullOrWhiteSpace(time) == false
                                    && new Regex(@"\d\d:\d\d").IsMatch(time)
                                    && TimeSpan.TryParseExact(time, "hh\\:mm", null, out var parsedDateTime))
                                   {
                                       timeStamp = parsedDate.Add(parsedDateTime);
                                       reminderMessage = message;
                                   }
                                   else if (new Regex(@"\d\d:\d\d").IsMatch(date)
                                         && TimeSpan.TryParseExact(date, "hh\\:mm", null, out var parsedTime))
                                   {
                                       timeStamp = DateTime.Today.Add(parsedTime);

                                       if (timeStamp.Value < DateTime.Now)
                                       {
                                           timeStamp = timeStamp.Value.AddDays(1);
                                       }

                                       reminderMessage = time;
                                       if (string.IsNullOrWhiteSpace(message) == false)
                                       {
                                           reminderMessage += " " + message;
                                       }
                                   }

                                   if (timeStamp != null)
                                   {
                                       await checkUser.ConfigureAwait(false);

                                       using (var dbFactory = RepositoryFactory.CreateInstance())
                                       {
                                           var reminderEntity = new OneTimeReminderEntity
                                                                {
                                                                    UserId = commandContext.User.Id,
                                                                    ChannelId = commandContext.Channel.Id,
                                                                    TimeStamp = timeStamp.Value,
                                                                    Message = reminderMessage
                                                                };

                                           if (dbFactory.GetRepository<OneTimeReminderRepository>()
                                                        .Add(reminderEntity))
                                           {
                                               JobScheduler.AddOneTimeReminder(reminderEntity.TimeStamp, reminderEntity.Id);

                                               await commandContext.Channel
                                                                   .SendMessageAsync(LocalizationGroup.GetFormattedText("ReminderCreated",
                                                                                                                        "The reminder has been set for {0} {1}.",
                                                                                                                        reminderEntity.TimeStamp
                                                                                                                                      .ToString(LocalizationGroup.CultureInfo.DateTimeFormat.ShortDatePattern,
                                                                                                                                                LocalizationGroup.CultureInfo.DateTimeFormat),
                                                                                                                        reminderEntity.TimeStamp
                                                                                                                                      .ToString(LocalizationGroup.CultureInfo.DateTimeFormat.ShortTimePattern,
                                                                                                                                                LocalizationGroup.CultureInfo.DateTimeFormat)))
                                                                   .ConfigureAwait(false);

                                               if (commandContext.Channel.IsPrivate == false)
                                               {
                                                   await commandContext.Channel
                                                                       .DeleteMessageAsync(commandContext.Message, LocalizationGroup.GetText("CommandProgressed", "Command progressed."))
                                                                       .ConfigureAwait(false);
                                               }
                                           }
                                       }
                                   }
                                   else
                                   {
                                       await commandContextContainer.ShowHelp("reminder at")
                                                                    .ConfigureAwait(false);
                                   }
                               });
        }

        /// <summary>
        /// Creation of a weekly reminder
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("weekly")]
        [RequireAdministratorPermissions]
        public Task RemindWeekly(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   var continueCreation = true;

                                   // Creation data
                                   var channelId = 0ul;
                                   var dayOfWeek = default(DayOfWeek);
                                   var postTimeSpan = default(TimeSpan);
                                   var deletionTimeSpan = default(TimeSpan);

                                   // Channel selection
                                   var messagesToBeDeleted = new List<DiscordMessage>
                                                             {
                                                                 commandContextContainer.Message
                                                             };

                                   var interactivity = commandContextContainer.Client.GetInteractivity();

                                   var stringBuilder = new StringBuilder();
                                   stringBuilder.Append(LocalizationGroup.GetText("ChooseChannelPrompt", "Please choose one of the following channels."));
                                   stringBuilder.Append("\n\n");

                                   var channels = new Dictionary<int, ulong>();

                                   var counter = 1;
                                   foreach (var (_, discordChannel) in commandContext.Guild.Channels.Where(obj => obj.Value.Type == ChannelType.Text))
                                   {
                                       channels[counter] = discordChannel.Id;

                                       stringBuilder.Append($"`{counter}` - {discordChannel.Mention}\n");
                                       counter++;
                                   }

                                   var embedBuilder = new DiscordEmbedBuilder
                                   {
                                       Color = DiscordColor.Green
                                   };

                                   embedBuilder.AddField(LocalizationGroup.GetText("ChannelSelectionTitle", "Channel selection"), stringBuilder.ToString());

                                   var currentBotMessage = await commandContextContainer.Message.RespondAsync(embedBuilder).ConfigureAwait(false);

                                   messagesToBeDeleted.Add(currentBotMessage);

                                   var userResponse = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContextContainer.Message.Author.Id).ConfigureAwait(false);
                                   if (userResponse.TimedOut == false)
                                   {
                                       messagesToBeDeleted.Add(userResponse.Result);

                                       continueCreation = int.TryParse(userResponse.Result.Content, out var parsedChannelId)
                                                       && channels.ContainsKey(parsedChannelId);

                                       if (continueCreation)
                                       {
                                           channelId = channels[parsedChannelId];
                                       }
                                   }
                                   else
                                   {
                                       continueCreation = false;
                                   }

                                   // Weekday selection
                                   if (continueCreation)
                                   {
                                       embedBuilder = new DiscordEmbedBuilder
                                       {
                                           Color = DiscordColor.Green
                                       };

                                       var reactions = new Dictionary<DiscordEmoji, DayOfWeek>
                                       {
                                           [DiscordEmoji.FromName(commandContext.Client, ":one:")] = DayOfWeek.Monday,
                                           [DiscordEmoji.FromName(commandContext.Client, ":two:")] = DayOfWeek.Tuesday,
                                           [DiscordEmoji.FromName(commandContext.Client, ":three:")] = DayOfWeek.Wednesday,
                                           [DiscordEmoji.FromName(commandContext.Client, ":four:")] = DayOfWeek.Thursday,
                                           [DiscordEmoji.FromName(commandContext.Client, ":five:")] = DayOfWeek.Friday,
                                           [DiscordEmoji.FromName(commandContext.Client, ":six:")] = DayOfWeek.Saturday,
                                           [DiscordEmoji.FromName(commandContext.Client, ":seven:")] = DayOfWeek.Sunday
                                       };

                                       stringBuilder.Clear();
                                       stringBuilder.Append(LocalizationGroup.GetText("SelectDayPrompt", "Please select one of the following days."));
                                       stringBuilder.Append("\n\n");

                                       foreach (var (emoji, day) in reactions)
                                       {
                                           stringBuilder.Append($"{emoji} {LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(day)}\n");
                                       }

                                       embedBuilder.AddField(LocalizationGroup.GetText("WeekdaySelectionTitle", "Weekday selection"), stringBuilder.ToString());

                                       currentBotMessage = await commandContext.RespondAsync(embedBuilder).ConfigureAwait(false);

                                       messagesToBeDeleted.Add(currentBotMessage);

                                       var dayOfWeekReactionTask = interactivity.WaitForReactionAsync(obj => obj.Message == currentBotMessage
                                                                                                            && reactions.ContainsKey(obj.Emoji),
                                                                                                  commandContext.User);

                                       foreach (var (emoji, _) in reactions)
                                       {
                                           await currentBotMessage.CreateReactionAsync(emoji).ConfigureAwait(false);
                                       }

                                       var dayOfWeekReaction = await dayOfWeekReactionTask.ConfigureAwait(false);

                                       continueCreation = dayOfWeekReaction.TimedOut == false
                                                       && reactions.TryGetValue(dayOfWeekReaction.Result.Emoji, out dayOfWeek);
                                   }

                                   // Post times
                                   if (continueCreation)
                                   {
                                       continueCreation = false;

                                       currentBotMessage = await commandContext.Channel.SendMessageAsync(LocalizationGroup.GetText("ReminderTimePrompt", "Please enter the reminder time. (Format: hh:mm)")).ConfigureAwait(false);

                                       messagesToBeDeleted.Add(currentBotMessage);

                                       userResponse = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContext.Message.Author.Id).ConfigureAwait(false);

                                       if (userResponse.TimedOut == false)
                                       {
                                           messagesToBeDeleted.Add(userResponse.Result);

                                           if (new Regex(@"\d\d:\d\d").IsMatch(userResponse.Result.Content))
                                           {
                                               postTimeSpan = TimeSpan.ParseExact(userResponse.Result.Content, "hh\\:mm", CultureInfo.InvariantCulture);

                                               continueCreation = true;
                                           }
                                       }
                                   }

                                   // Deletion time
                                   if (continueCreation)
                                   {
                                       continueCreation = false;

                                       currentBotMessage = await commandContext.Channel.SendMessageAsync(LocalizationGroup.GetText("DeletionTimePrompt", "Please enter the deletion times. (Format: hh:mm)")).ConfigureAwait(false);

                                       messagesToBeDeleted.Add(currentBotMessage);

                                       userResponse = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContext.Message.Author.Id).ConfigureAwait(false);

                                       if (userResponse.TimedOut == false)
                                       {
                                           messagesToBeDeleted.Add(userResponse.Result);

                                           if (new Regex(@"\d\d:\d\d").IsMatch(userResponse.Result.Content))
                                           {
                                               deletionTimeSpan = TimeSpan.ParseExact(userResponse.Result.Content, "hh\\:mm", CultureInfo.InvariantCulture);

                                               continueCreation = deletionTimeSpan > postTimeSpan;
                                           }
                                       }
                                   }

                                   // Creation
                                   if (continueCreation)
                                   {
                                       currentBotMessage = await commandContext.Channel.SendMessageAsync(LocalizationGroup.GetText("ReminderMessagePrompt", "Please enter the message of the reminder.")).ConfigureAwait(false);

                                       messagesToBeDeleted.Add(currentBotMessage);

                                       userResponse = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContext.Message.Author.Id).ConfigureAwait(false);

                                       if (userResponse.TimedOut == false)
                                       {
                                           messagesToBeDeleted.Add(userResponse.Result);

                                           using (var dbFactory = RepositoryFactory.CreateInstance())
                                           {
                                               var entity = new WeeklyReminderEntity
                                               {
                                                   DayOfWeek = dayOfWeek,
                                                   ChannelId = channelId,
                                                   PostTime = postTimeSpan,
                                                   DeletionTime = deletionTimeSpan,
                                                   Message = userResponse.Result.Content
                                               };

                                               if (dbFactory.GetRepository<WeeklyReminderRepository>()
                                                            .Add(entity))
                                               {
                                                   JobScheduler.AddWeeklyReminder(entity.Id, entity.DayOfWeek, entity.PostTime, entity.DeletionTime);

                                                   currentBotMessage = await commandContext.Channel.SendMessageAsync(LocalizationGroup.GetText("CreationCompletedMessage", "Creation completed! All creation messages will be deleted in 30 seconds.")).ConfigureAwait(false);

                                                   messagesToBeDeleted.Add(currentBotMessage);

                                                   await Task.Delay(TimeSpan.FromSeconds(30)).ConfigureAwait(false);
                                               }
                                           }
                                       }
                                   }

                                   await commandContext.Channel
                                                       .DeleteMessagesAsync(messagesToBeDeleted)
                                                       .ConfigureAwait(false);
                               });
        }

        #endregion // Command methods
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using Scruffy.Commands.Base;
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Reminder;
using Scruffy.Data.Entity.Tables.Reminder;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.CoreData;

namespace Scruffy.Commands.Reminder
{
    /// <summary>
    /// Reminder module
    /// </summary>
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class ReminderCreationCommandModule : LocatedCommandModuleBase
    {
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

        #region Command methods

        /// <summary>
        /// Creation of a one time reminder
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="timeSpan">Timespan until the reminder should be executed.</param>
        /// <param name="message">Optional message of the reminder</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("remindme")]
        public async Task RemindMe(CommandContext commandContext, string timeSpan, [RemainingText] string message = null)
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
                                            .DeleteMessageAsync(commandContext.Message, LocalizationGroup.GetText("CommandProgressed", "Command progressed."))
                                            .ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Creation of a weekly reminder
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("remindweekly")]
        public async Task RemindWeekly(CommandContext commandContext)
        {
            var messagesToBeDeleted = new List<DiscordMessage>
                                      {
                                          commandContext.Message
                                      };

            var interactivity = commandContext.Client.GetInteractivity();

            var embedBuilder = new DiscordEmbedBuilder
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

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(LocalizationGroup.GetText("SelectDayPrompt", "Please select one of the following days."));
            stringBuilder.Append("\n\n");

            foreach (var (emoji, day) in reactions)
            {
                stringBuilder.Append($"{emoji} {DateTimeFormatInfo.CurrentInfo.GetDayName(day)}\n");
            }

            embedBuilder.AddField(LocalizationGroup.GetText("WeekdaySelectionTitle", "Weekday selection"), stringBuilder.ToString());

            var currentBotMessage = await commandContext.RespondAsync(embedBuilder).ConfigureAwait(false);

            messagesToBeDeleted.Add(currentBotMessage);

            var dayOfWeekReactionTask = interactivity.WaitForReactionAsync(obj => obj.Message == currentBotMessage
                                                                                 && reactions.ContainsKey(obj.Emoji),
                                                                       commandContext.User);

            foreach (var (emoji, day) in reactions)
            {
                await currentBotMessage.CreateReactionAsync(emoji).ConfigureAwait(false);
            }

            var dayOfWeekReaction = await dayOfWeekReactionTask.ConfigureAwait(false);

            if (dayOfWeekReaction.TimedOut == false
             && reactions.TryGetValue(dayOfWeekReaction.Result.Emoji, out var dayOfWeek))
            {
                TimeSpan postTimeSpan = default;
                TimeSpan deletionTimeSpan = default;

                currentBotMessage = await commandContext.Channel.SendMessageAsync(LocalizationGroup.GetText("ReminderTimePrompt", "Please enter the reminder time. (Format: hh:mm)")).ConfigureAwait(false);

                messagesToBeDeleted.Add(currentBotMessage);

                var userResponse = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContext.Message.Author.Id).ConfigureAwait(false);

                if (userResponse.TimedOut == false)
                {
                    messagesToBeDeleted.Add(userResponse.Result);

                    if (new Regex(@"\d\d:\d\d").IsMatch(userResponse.Result.Content))
                    {
                        postTimeSpan = TimeSpan.ParseExact(userResponse.Result.Content, "hh\\:mm", CultureInfo.InvariantCulture);

                        currentBotMessage = await commandContext.Channel.SendMessageAsync(LocalizationGroup.GetText("DeletionTimePrompt", "Please enter the deletion times. (Format: hh:mm)")).ConfigureAwait(false);

                        messagesToBeDeleted.Add(currentBotMessage);

                        userResponse = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContext.Message.Author.Id).ConfigureAwait(false);
                    }
                }

                if (userResponse.TimedOut == false)
                {
                    messagesToBeDeleted.Add(userResponse.Result);

                    if (new Regex(@"\d\d:\d\d").IsMatch(userResponse.Result.Content))
                    {
                        deletionTimeSpan = TimeSpan.ParseExact(userResponse.Result.Content, "hh\\:mm", CultureInfo.InvariantCulture);

                        currentBotMessage = await commandContext.Channel.SendMessageAsync(LocalizationGroup.GetText("ReminderMessagePrompt", "Please enter the message of the reminder.")).ConfigureAwait(false);

                        messagesToBeDeleted.Add(currentBotMessage);

                        userResponse = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContext.Message.Author.Id).ConfigureAwait(false);
                    }
                }

                if (userResponse.TimedOut == false)
                {
                    messagesToBeDeleted.Add(userResponse.Result);

                    using (var dbFactory = RepositoryFactory.CreateInstance())
                    {
                        var entity = new WeeklyReminderEntity
                                     {
                                         DayOfWeek = dayOfWeek,
                                         ChannelId = commandContext.Channel.Id,
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

            foreach (var message in messagesToBeDeleted)
            {
                await commandContext.Channel.DeleteMessageAsync(message).ConfigureAwait(false);
            }
        }

        #endregion // Command methods
    }
}

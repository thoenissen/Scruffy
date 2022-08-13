using System.Globalization;
using System.Text.RegularExpressions;

using Discord;
using Discord.Commands;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Reminder;
using Scruffy.Data.Entity.Tables.Reminder;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord.Attributes;
using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.Services.Reminder;

/// <summary>
/// Reminder commands
/// </summary>
public class ReminderCommandHandler : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// User management service
    /// </summary>
    private readonly UserManagementService _userManagementService;

    /// <summary>
    /// Job scheduler
    /// </summary>
    private readonly JobScheduler _jobScheduler;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="userManagementService">User management service</param>
    /// <param name="jobScheduler">Job scheduler</param>
    public ReminderCommandHandler(LocalizationService localizationService, UserManagementService userManagementService, JobScheduler jobScheduler)
        : base(localizationService)
    {
        _userManagementService = userManagementService;
        _jobScheduler = jobScheduler;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Creation of a one time reminder
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="message">Message of the reminder</param>
    /// <param name="timeSpan">Timespan until the reminder should be executed.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ReminderIn(IContextContainer context, string message, string timeSpan)
    {
        var checkUser = _userManagementService.CheckDiscordAccountAsync(context.User);

        var timeSpanValidation = new Regex(@"\d+(h|m|s)");
        if (timeSpanValidation.IsMatch(timeSpan))
        {
            var amount = Convert.ToUInt64(timeSpan[..^1], CultureInfo.InvariantCulture);

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                await checkUser.ConfigureAwait(false);

                var reminderEntity = new OneTimeReminderEntity
                                     {
                                         DiscordAccountId = context.User.Id,
                                         DiscordChannelId = context.Channel.Id,
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
                    _jobScheduler.AddOneTimeReminder(reminderEntity.TimeStamp, reminderEntity.Id);

                    await context.ReplyAsync(LocalizationGroup.GetFormattedText("ReminderCreated",
                                                                                "The reminder has been set for {0} {1}.",
                                                                                reminderEntity.TimeStamp
                                                                                              .ToString(LocalizationGroup.CultureInfo.DateTimeFormat.ShortDatePattern,
                                                                                                        LocalizationGroup.CultureInfo.DateTimeFormat),
                                                                                reminderEntity.TimeStamp
                                                                                              .ToString(LocalizationGroup.CultureInfo.DateTimeFormat.ShortTimePattern,
                                                                                                        LocalizationGroup.CultureInfo.DateTimeFormat)),
                                             ephemeral: true)
                                 .ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Creation of a one time reminder
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="message">Message of the reminder</param>
    /// <param name="date">Date</param>
    /// <param name="time">Time</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("at")]
    public async Task ReminderAt(IContextContainer context, string message, string date, string time)
    {
        var checkUser = _userManagementService.CheckDiscordAccountAsync(context.User);

        DateTime? timeStamp = null;

        if (string.IsNullOrWhiteSpace(date) == false)
        {
            if (new Regex(@"\d\d\d\d-\d\d-\d\d").IsMatch(date)
             && DateTime.TryParseExact(date,
                                       "yyyy-MM-dd",
                                       null,
                                       DateTimeStyles.None,
                                       out var parsedDate)
             && string.IsNullOrWhiteSpace(time) == false
             && new Regex(@"\d\d:\d\d").IsMatch(time)
             && TimeSpan.TryParseExact(time, "hh\\:mm", null, out var parsedDateTime))
            {
                timeStamp = parsedDate.Add(parsedDateTime);
            }
        }
        else
        {
            if (new Regex(@"\d\d:\d\d").IsMatch(time)
             && TimeSpan.TryParseExact(time, "hh\\:mm", null, out var parsedTime))
            {
                timeStamp = DateTime.Today.Add(parsedTime);

                if (timeStamp.Value < DateTime.Now)
                {
                    timeStamp = timeStamp.Value.AddDays(1);
                }
            }
        }

        if (timeStamp != null)
        {
            await checkUser.ConfigureAwait(false);

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var reminderEntity = new OneTimeReminderEntity
                                     {
                                         DiscordAccountId = context.User.Id,
                                         DiscordChannelId = context.Channel.Id,
                                         TimeStamp = timeStamp.Value,
                                         Message = message
                                     };

                if (dbFactory.GetRepository<OneTimeReminderRepository>()
                             .Add(reminderEntity))
                {
                    _jobScheduler.AddOneTimeReminder(reminderEntity.TimeStamp, reminderEntity.Id);

                    await context.ReplyAsync(LocalizationGroup.GetFormattedText("ReminderCreated",
                                                                                "The reminder has been set for {0} {1}.",
                                                                                reminderEntity.TimeStamp
                                                                                              .ToString(LocalizationGroup.CultureInfo.DateTimeFormat.ShortDatePattern,
                                                                                                        LocalizationGroup.CultureInfo.DateTimeFormat),
                                                                                reminderEntity.TimeStamp
                                                                                              .ToString(LocalizationGroup.CultureInfo.DateTimeFormat.ShortTimePattern,
                                                                                                        LocalizationGroup.CultureInfo.DateTimeFormat)),
                                             ephemeral: true)
                                 .ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Creation of a weekly reminder
    /// </summary>
    /// <param name="context">Context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("weekly")]
    [RequireAdministratorPermissions]
    public async Task WeeklyReminderConfiguration(IContextContainer context)
    {
        var continueCreation = true;

        // Creation data
        var channelId = 0ul;
        var dayOfWeek = default(DayOfWeek);
        var postTimeSpan = default(TimeSpan);
        var deletionTimeSpan = default(TimeSpan);

        // Channel selection
        var messagesToBeDeleted = new List<IUserMessage>();

        var stringBuilder = new StringBuilder();
        stringBuilder.Append(LocalizationGroup.GetText("ChooseChannelPrompt", "Please choose one of the following channels."));
        stringBuilder.Append("\n\n");

        var channels = new Dictionary<int, ulong>();

        var counter = 1;

        foreach (var discordChannel in await context.Guild.GetTextChannelsAsync().ConfigureAwait(false))
        {
            channels[counter] = discordChannel.Id;

            stringBuilder.Append($"`{counter}` - {discordChannel.Mention}\n");
            counter++;
        }

        var embedBuilder = new EmbedBuilder
                           {
                               Color = Color.Green
                           };

        embedBuilder.AddField(LocalizationGroup.GetText("ChannelSelectionTitle", "Channel selection"), stringBuilder.ToString());

        var currentBotMessage = await context.ReplyAsync(embed: embedBuilder.Build())
                                             .ConfigureAwait(false);

        messagesToBeDeleted.Add(currentBotMessage);

        var userResponse = await context.Interactivity
                                        .WaitForMessageAsync(obj => obj.Author.Id == context.User.Id)
                                        .ConfigureAwait(false);

        if (userResponse != null)
        {
            messagesToBeDeleted.Add(userResponse);

            continueCreation = int.TryParse(userResponse.Content, out var parsedChannelId)
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
            embedBuilder = new EmbedBuilder
                           {
                               Color = Color.Green
                           };

            var reactions = new Dictionary<Emoji, DayOfWeek>
                            {
                                [Emoji.Parse(":one:")] = DayOfWeek.Monday,
                                [Emoji.Parse(":two:")] = DayOfWeek.Tuesday,
                                [Emoji.Parse(":three:")] = DayOfWeek.Wednesday,
                                [Emoji.Parse(":four:")] = DayOfWeek.Thursday,
                                [Emoji.Parse(":five:")] = DayOfWeek.Friday,
                                [Emoji.Parse(":six:")] = DayOfWeek.Saturday,
                                [Emoji.Parse(":seven:")] = DayOfWeek.Sunday
                            };

            stringBuilder.Clear();
            stringBuilder.Append(LocalizationGroup.GetText("SelectDayPrompt", "Please select one of the following days."));
            stringBuilder.Append("\n\n");

            foreach (var (emoji, day) in reactions)
            {
                stringBuilder.Append($"{emoji} {LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(day)}\n");
            }

            embedBuilder.AddField(LocalizationGroup.GetText("WeekdaySelectionTitle", "Weekday selection"), stringBuilder.ToString());

            currentBotMessage = await context.SendMessageAsync(embed: embedBuilder.Build())
                                             .ConfigureAwait(false);

            messagesToBeDeleted.Add(currentBotMessage);

            var dayOfWeekReactionTask = context.Interactivity
                                               .WaitForReactionAsync(currentBotMessage, context.User);

            foreach (var (emoji, _) in reactions)
            {
                await currentBotMessage.AddReactionAsync(emoji)
                                       .ConfigureAwait(false);
            }

            var dayOfWeekReaction = await dayOfWeekReactionTask.ConfigureAwait(false);

            continueCreation = dayOfWeekReaction?.Emote is Emoji
                            && reactions.TryGetValue(dayOfWeekReaction.Emote.Name, out dayOfWeek);
        }

        // Post times
        if (continueCreation)
        {
            continueCreation = false;

            currentBotMessage = await context.Channel
                                             .SendMessageAsync(LocalizationGroup.GetText("ReminderTimePrompt", "Please enter the reminder time. (Format: hh:mm)"))
                                             .ConfigureAwait(false);

            messagesToBeDeleted.Add(currentBotMessage);

            userResponse = await context.Interactivity
                                        .WaitForMessageAsync(obj => obj.Author.Id == context.User.Id)
                                        .ConfigureAwait(false);

            if (userResponse != null)
            {
                messagesToBeDeleted.Add(userResponse);

                if (new Regex(@"\d\d:\d\d").IsMatch(userResponse.Content))
                {
                    postTimeSpan = TimeSpan.ParseExact(userResponse.Content, "hh\\:mm", CultureInfo.InvariantCulture);

                    continueCreation = true;
                }
            }
        }

        // Deletion time
        if (continueCreation)
        {
            continueCreation = false;

            currentBotMessage = await context.Channel
                                             .SendMessageAsync(LocalizationGroup.GetText("DeletionTimePrompt", "Please enter the deletion times. (Format: hh:mm)"))
                                             .ConfigureAwait(false);

            messagesToBeDeleted.Add(currentBotMessage);

            userResponse = await context.Interactivity
                                        .WaitForMessageAsync(obj => obj.Author.Id == context.User.Id)
                                        .ConfigureAwait(false);

            if (userResponse != null)
            {
                messagesToBeDeleted.Add(userResponse);

                if (new Regex(@"\d\d:\d\d").IsMatch(userResponse.Content))
                {
                    deletionTimeSpan = TimeSpan.ParseExact(userResponse.Content, "hh\\:mm", CultureInfo.InvariantCulture);

                    continueCreation = deletionTimeSpan > postTimeSpan;
                }
            }
        }

        // Creation
        if (continueCreation)
        {
            currentBotMessage = await context.Channel
                                             .SendMessageAsync(LocalizationGroup.GetText("ReminderMessagePrompt", "Please enter the message of the reminder."))
                                             .ConfigureAwait(false);

            messagesToBeDeleted.Add(currentBotMessage);

            userResponse = await context.Interactivity
                                        .WaitForMessageAsync(obj => obj.Author.Id == context.User.Id)
                                        .ConfigureAwait(false);

            if (userResponse != null)
            {
                messagesToBeDeleted.Add(userResponse);

                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    var entity = new WeeklyReminderEntity
                                 {
                                     DayOfWeek = dayOfWeek,
                                     DiscordChannelId = channelId,
                                     PostTime = postTimeSpan,
                                     DeletionTime = deletionTimeSpan,
                                     Message = userResponse.Content
                                 };

                    if (dbFactory.GetRepository<WeeklyReminderRepository>()
                                 .Add(entity))
                    {
                        _jobScheduler.AddWeeklyReminder(entity.Id, entity.DayOfWeek, entity.PostTime, entity.DeletionTime);

                        currentBotMessage = await context.Channel
                                                         .SendMessageAsync(LocalizationGroup.GetText("CreationCompletedMessage", "Creation completed! All creation messages will be deleted in 30 seconds."))
                                                         .ConfigureAwait(false);

                        messagesToBeDeleted.Add(currentBotMessage);

                        await Task.Delay(TimeSpan.FromSeconds(30))
                                  .ConfigureAwait(false);
                    }
                }
            }
        }

        if (context.Channel is ITextChannel textChannel)
        {
            await textChannel.DeleteMessagesAsync(messagesToBeDeleted)
                             .ConfigureAwait(false);
        }
    }

    #endregion // Methods
}
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
using Scruffy.Services.Discord;
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
    /// List reminders
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ListReminders(IContextContainer context)
    {
        var message = await context.DeferProcessing(true)
                                   .ConfigureAwait(false);

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var reminders = dbFactory.GetRepository<OneTimeReminderRepository>()
                                     .GetQuery()
                                     .Where(reminder => reminder.DiscordAccountId == context.User.Id
                                                        && reminder.IsExecuted == false)
                                     .OrderBy(reminder => reminder.TimeStamp)
                                     .Select(reminder => new
                                                         {
                                                             reminder.TimeStamp,
                                                             reminder.Message
                                                         })
                                     .Take(50)
                                     .ToList();

            if (reminders.Count > 0)
            {
                var embedBuilder = new EmbedBuilder().WithTitle(LocalizationGroup.GetText("ListRemindersTitle", "Open reminders"))
                                                     .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                                     .WithColor(Color.Green)
                                                     .WithTimestamp(DateTime.Now);

                var descriptionBuilder = new StringBuilder();
                var lineBuilder = new StringBuilder();

                foreach (var reminder in reminders)
                {
                    lineBuilder.Clear();
                    lineBuilder.Append("**");
                    lineBuilder.Append(reminder.TimeStamp.ToString("g", LocalizationGroup.CultureInfo));
                    lineBuilder.Append("** `");

                    if (reminder.Message.Length > 30)
                    {
                        lineBuilder.Append(reminder.Message[..27]);
                        lineBuilder.Append("...");
                    }
                    else
                    {
                        lineBuilder.Append(reminder.Message);
                    }

                    lineBuilder.AppendLine("`");

                    if (descriptionBuilder.Length + lineBuilder.Length > 4093)
                    {
                        descriptionBuilder.Append("...");

                        break;
                    }

                    descriptionBuilder.Append(lineBuilder);
                }

                if (reminders.Count == 50
                    && descriptionBuilder[reminders.Count - 1] != '.')
                {
                    descriptionBuilder.Append("...");
                }

                embedBuilder.Description = descriptionBuilder.ToString();

                await context.ReplyAsync(embed: embedBuilder.Build())
                             .ConfigureAwait(false);
            }
            else
            {
                await context.ReplyAsync(LocalizationGroup.GetFormattedText("NoOpenReminders", "There are no open reminders."))
                             .ConfigureAwait(false);
            }
        }
    }

    #endregion // Methods
}
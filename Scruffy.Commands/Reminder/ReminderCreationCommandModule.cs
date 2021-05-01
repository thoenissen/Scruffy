using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Reminder;
using Scruffy.Data.Entity.Tables.Reminder;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.CoreData;

namespace Scruffy.Commands.Reminder
{
    /// <summary>
    /// Reminder module
    /// </summary>
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class ReminderCreationCommandModule : BaseCommandModule
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
                                            .DeleteMessageAsync(commandContext.Message, "Command progressed.")
                                            .ConfigureAwait(false);
                    }
                }
            }
        }

        #endregion // Command methods
    }
}

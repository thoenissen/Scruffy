using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Reminder;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Reminder
{
    /// <summary>
    /// Execution of a one time reminder
    /// </summary>
    public class OneTimeReminderJob : AsyncJob
    {
        private long _id;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Entity id</param>
        public OneTimeReminderJob(long id)
        {
            _id = id;
        }

        #region IJob

        /// <summary>
        /// Executes the job.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected override async Task ExecuteAsync()
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var jobEntity = dbFactory.GetRepository<OneTimeReminderRepository>()
                                         .GetQuery()
                                         .FirstOrDefault(obj => obj.Id == _id);

                if (jobEntity != null)
                {
                    await using (var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider())
                    {
                        var discordClient = serviceProvider.GetService<DiscordClient>();

                        var channel = await discordClient.GetChannelAsync(jobEntity.ChannelId);

                        if (channel != null)
                        {
                            var user = await discordClient.GetUserAsync(jobEntity.UserId);

                            await discordClient.SendMessageAsync(channel,
                                                                 new DiscordMessageBuilder
                                                                 {
                                                                     Content = $"{user.Mention} - {(string.IsNullOrWhiteSpace(jobEntity.Message) ? "Reminder!" : jobEntity.Message)}"
                                                                 });

                            dbFactory.GetRepository<OneTimeReminderRepository>()
                                     .Refresh(obj => obj.Id == _id,
                                              obj => obj.IsExecuted = true);
                        }
                    }
                }
            }
        }

        #endregion // IJob
    }
}

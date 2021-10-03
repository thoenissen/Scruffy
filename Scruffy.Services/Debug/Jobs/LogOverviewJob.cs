using System;
using System.Threading.Tasks;

using DSharpPlus;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Debug.Jobs
{
    /// <summary>
    /// Posting the log overview
    /// </summary>
    public class LogOverviewJob : LocatedAsyncJob
    {
        #region LocatedAsyncJob

        /// <summary>
        /// Executes the job
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task ExecuteAsync()
        {
            var debugChannel = Environment.GetEnvironmentVariable("SCRUFFY_DEBUG_CHANNEL");
            if (string.IsNullOrWhiteSpace(debugChannel) == false)
            {
                await using (var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider())
                {
                    var discordClient = serviceProvider.GetService<DiscordClient>();
                    var debugService = serviceProvider.GetService<DebugService>();

                    await debugService.PostLogOverview(await discordClient.GetChannelAsync(Convert.ToUInt64(debugChannel))
                                                                          .ConfigureAwait(false),
                                                       DateTime.Today.AddDays(-1),
                                                       true)
                                      .ConfigureAwait(false);
                }
            }
        }

        #endregion // LocatedAsyncJob
    }
}

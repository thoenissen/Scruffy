using System;
using System.Threading.Tasks;

using Scruffy.ServiceHost.Discord;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.ServiceHost
{
    /// <summary>
    /// Main class
    /// </summary>
    public class Program
    {
        #region Fields

        /// <summary>
        /// Wait for program exit
        /// </summary>
        private static TaskCompletionSource<bool> _waitForExitTaskSource = new TaskCompletionSource<bool>();

        #endregion // Fields

        #region Methods

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <returns>Task</returns>
        public static async Task Main()
        {
            Console.CancelKeyPress += OnCancelKeyPress;

            await using (var jobScheduler = new JobScheduler())
            {
                GlobalServiceProvider.Current.AddSingleton(jobScheduler);

                await using (var discordBot = new DiscordBot())
                {
                    await discordBot.StartAsync();

                    await jobScheduler.StartAsync();

                    await _waitForExitTaskSource.Task;
                }
            }
        }

        /// <summary>
        /// The cancel key was pressed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e) => _waitForExitTaskSource.SetResult(true);

        #endregion // Methods
    }
}

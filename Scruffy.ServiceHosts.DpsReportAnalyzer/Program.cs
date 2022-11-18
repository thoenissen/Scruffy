using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Enumerations.General;
using Scruffy.ServiceHosts.DpsReportAnalyzer.Services;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;

namespace Scruffy.ServiceHosts.DpsReportAnalyzer
{
    /// <summary>
    /// Main class
    /// </summary>
    internal class Program
    {
        #region Fields

        /// <summary>
        /// Wait for program exit
        /// </summary>
        private static TaskCompletionSource<bool> _waitForExitTaskSource = new();

        #endregion // Fields

        #region Methods

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <returns>Task</returns>
        public static async Task Main()
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            try
            {
                LoggingService.Initialize();
                LoggingService.AddServiceLogEntry(LogEntryLevel.Information, nameof(Program), "Start", null);

                var localizationService = new LocalizationService();
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddTransient<DpsReportConnector>();
                serviceCollection.AddTransient<DpsReportImporter>();
                serviceCollection.AddSingleton(localizationService);
                serviceCollection.AddHttpClient();

                var scheduler = new JobScheduler();
                await using (scheduler.ConfigureAwait(false))
                {
                    var provider = serviceCollection.BuildServiceProvider();

                    await localizationService.Initialize(provider)
                                             .ConfigureAwait(false);

                    await scheduler.Initialize(provider)
                                   .ConfigureAwait(false);

                    await scheduler.StartAsync()
                                   .ConfigureAwait(false);

                    await _waitForExitTaskSource.Task.ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LoggingService.AddServiceLogEntry(LogEntryLevel.CriticalError, nameof(Program), null, null, ex);
            }
            finally
            {
                LoggingService.AddServiceLogEntry(LogEntryLevel.Information, nameof(Program), "End", null);
            }
        }

        /// <summary>
        /// The cancel key was pressed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = false;

            _waitForExitTaskSource.SetResult(true);
        }

        /// <summary>
        /// Occurs when the default application domain's parent process exits.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Argument</param>
        private static void OnProcessExit(object sender, EventArgs e) => _waitForExitTaskSource.SetResult(true);

        #endregion // Methods
    }
}
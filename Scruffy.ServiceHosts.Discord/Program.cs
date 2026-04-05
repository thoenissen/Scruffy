using System.Threading;

using Scruffy.Data.Enumerations.General;
using Scruffy.ServiceHosts.Discord.Discord;
using Scruffy.ServiceHosts.Discord.Endpoints;
using Scruffy.Services.Core;

namespace Scruffy.ServiceHosts.Discord;

/// <summary>
/// Main class
/// </summary>
public class Program
{
    #region Fields

    /// <summary>
    /// Wait for program exit
    /// </summary>
    private static readonly TaskCompletionSource<bool> WaitForExitTaskSource = new();

    /// <summary>
    /// Signals that the shutdown has completed
    /// </summary>
    private static readonly ManualResetEventSlim ShutdownComplete = new(false);

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

            var discordBot = new DiscordBot();

            await using (discordBot.ConfigureAwait(false))
            {
                await discordBot.StartAsync()
                                .ConfigureAwait(false);

                var webApp = RestApiHost.Create();

                await using (webApp.ConfigureAwait(false))
                {
                    await webApp.StartAsync()
                                .ConfigureAwait(false);

                    await WaitForExitTaskSource.Task.ConfigureAwait(false);

                    await webApp.StopAsync()
                                .ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.CriticalError, nameof(Program), null, null, ex);
        }
        finally
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Information, nameof(Program), "End", null);

            ShutdownComplete.Set();
        }
    }

    /// <summary>
    /// The cancel key was pressed
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Arguments</param>
    private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        LoggingService.AddServiceLogEntry(LogEntryLevel.Information, nameof(Program), "OnCancelKeyPress", null);

        e.Cancel = true;

        WaitForExitTaskSource.TrySetResult(true);
    }

    /// <summary>
    /// Occurs when the default application domain's parent process exits.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Argument</param>
    private static void OnProcessExit(object sender, EventArgs e)
    {
        LoggingService.AddServiceLogEntry(LogEntryLevel.Information, nameof(Program), "OnProcessExit", null);

        WaitForExitTaskSource.TrySetResult(true);

        ShutdownComplete.Wait(TimeSpan.FromSeconds(30));
    }

    #endregion // Methods
}
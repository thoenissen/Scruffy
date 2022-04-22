using Discord.Commands;

using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Reminder module
/// </summary>
[BlockedChannelCheck]
[Group("reminder")]
[Alias("re")]
public class ReminderCreationCommandModule : LocatedTextCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Scheduling jobs
    /// </summary>
    public JobScheduler JobScheduler { get; set; }

    #endregion // Properties

    #region Command methods

    /// <summary>
    /// Creation of a one time reminder
    /// </summary>
    /// <param name="timeSpan">Timespan until the reminder should be executed.</param>
    /// <param name="message">Optional message of the reminder</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("in")]
    public Task RemindMeIn(string timeSpan, [Remainder] string message = null) => ShowMigrationMessage("reminder in");

    /// <summary>
    /// Creation of a one time reminder
    /// </summary>
    /// <param name="time">Time</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("at")]
    public Task RemindMeAt(string time) => ShowMigrationMessage("reminder at");

    /// <summary>
    /// Creation of a one time reminder
    /// </summary>
    /// <param name="time">Time</param>
    /// <param name="message">Optional message of the reminder</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("at")]
    public Task RemindMeAt(string time, string message) => ShowMigrationMessage("reminder at");

    /// <summary>
    /// Creation of a one time reminder
    /// </summary>
    /// <param name="date">Date</param>
    /// <param name="time">Time</param>
    /// <param name="message">Optional message of the reminder</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("at")]
    public Task RemindMeAt(string date, string time, [Remainder] string message) => ShowMigrationMessage("reminder at");

    /// <summary>
    /// Creation of a weekly reminder
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("weekly")]
    [RequireAdministratorPermissions]
    public Task RemindWeekly() => ShowMigrationMessage("reminder-admin configuration");

    #endregion // Command methods
}
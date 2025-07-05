﻿using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.Reminder;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Reminder commands
/// </summary>
[Group("reminder", "Reminder creation")]
public class ReminderSlashCommandModule : SlashCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public ReminderCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Reminder creation
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="timeSpan">Timespan</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("in", "Creation of a reminder which will executed after the given timespan.")]
    public async Task ReminderIn([Summary("Message", "Message of the reminder")] string message,
                                 [Summary("Timespan", "Timespan to wait to post the reminder (xh|m|s)")] string timeSpan)
    {
        await CommandHandler.ReminderIn(Context, message, timeSpan)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Reminder creation
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="time">Time</param>
    /// <param name="date">Date</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("at", "Creation of a reminder which will executed at the given time.")]
    public async Task ReminderAt([Summary("Message", "Message of the reminder")]string message,
                                 [Summary("Time", "Time of the reminder (hh:mm)")]string time,
                                 [Summary("Date", "Date of the reminder (yyyy-MM-dd)")]string date = null)
    {
        await CommandHandler.ReminderAt(Context, message, date, time)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// List reminders
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("list", "Lists the next open reminders.")]
    public async Task ListReminders()
    {
        await CommandHandler.ListReminders(Context)
                            .ConfigureAwait(false);
    }

    #endregion // Methods
}
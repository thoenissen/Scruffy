using Discord;
using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.Reminder;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Reminder commands
/// </summary>
[Group("reminder-admin", "Reminder creation")]
[DefaultMemberPermissions(GuildPermission.Administrator)]
public class ReminderAdminSlashCommandModule : SlashCommandModuleBase
{
    #region Enumerations

    /// <summary>
    /// Configuration types
    /// </summary>
    public enum ReminderConfigurationType
    {
        Weekly
    }

    #endregion // Enumerations

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
    /// <param name="type">Type</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("configuration", "Reminder configuration")]
    public async Task ReminderConfiguration([Summary("Type", "Type of the configuration")]ReminderConfigurationType type)
    {
        switch (type)
        {
            case ReminderConfigurationType.Weekly:
                {
                    await CommandHandler.WeeklyReminderConfiguration(Context)
                                        .ConfigureAwait(false);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    #endregion // Methods
}
using Discord.Interactions;

using Scruffy.Services.Calendar;
using Scruffy.Services.Discord;

namespace Scruffy.Commands.SlashCommands
{
    /// <summary>
    /// Calendar admin commands
    /// </summary>
    [Group("calendar-admin", "Calendar administration commands")]
    public class CalendarAdminSlashCommandModule : SlashCommandModuleBase
    {
        #region Enumerations

        /// <summary>
        /// Configuration types
        /// </summary>
        public enum CalendarConfigurationType
        {
            [ChoiceDisplay("Templates")]
            Templates,
            [ChoiceDisplay("Schedules")]
            Schedules,
            [ChoiceDisplay("One time event")]
            OneTimeEvent
        }

        #endregion // Enumerations

        #region Properties

        /// <summary>
        /// Command handler
        /// </summary>
        public CalendarCommandHandler CommandHandler { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Set participants
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [SlashCommand("participants", "Set the participants of a event")]
        public Task SetParticipants() => CommandHandler.SetParticipants(Context);

        /// <summary>
        /// Configuration
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [SlashCommand("configuration", "Calendar configuration")]
        public async Task Configuration([Summary("Type", "Configuration type")]CalendarConfigurationType type)
        {
            switch (type)
            {
                case CalendarConfigurationType.Templates:
                    {
                        await CommandHandler.ConfigureTemplates(Context)
                                            .ConfigureAwait(false);
                    }
                    break;

                case CalendarConfigurationType.Schedules:
                    {
                        await CommandHandler.ConfigureSchedules(Context)
                                            .ConfigureAwait(false);
                    }
                    break;

                case CalendarConfigurationType.OneTimeEvent:
                    {
                        await CommandHandler.AddOneTimeEvent(Context)
                                            .ConfigureAwait(false);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        #endregion // Methods
    }
}
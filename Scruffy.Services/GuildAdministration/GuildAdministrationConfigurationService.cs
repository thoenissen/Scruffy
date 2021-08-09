using System.Threading.Tasks;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildAdministration;
using Scruffy.Services.Calendar;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.GuildAdministration.DialogElements.Forms;

namespace Scruffy.Services.GuildAdministration
{
    /// <summary>
    /// Configuration of the guild administration
    /// </summary>
    public class GuildAdministrationConfigurationService : LocatedServiceBase
    {
        #region Fields

        /// <summary>
        /// Calendar message builder
        /// </summary>
        private readonly CalendarMessageBuilderService _calendarMessageBuilderService;

        /// <summary>
        /// Calendar scheduling
        /// </summary>
        private readonly CalendarScheduleService _calendarScheduleService;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="calendarMessageBuilderService">Message builder</param>
        /// <param name="calendarScheduleService">Schedule service</param>
        public GuildAdministrationConfigurationService(LocalizationService localizationService, CalendarMessageBuilderService calendarMessageBuilderService, CalendarScheduleService calendarScheduleService)
            : base(localizationService)
        {
            _calendarMessageBuilderService = calendarMessageBuilderService;
            _calendarScheduleService = calendarScheduleService;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Create a new guild configuration
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CreateGuildConfiguration(CommandContextContainer commandContext)
        {
            var data = await DialogHandler.RunForm<CreateGuildAdministrationFormData>(commandContext, true)
                                          .ConfigureAwait(false);

            if (data != null)
            {
                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    dbFactory.GetRepository<GuildRepository>()
                             .AddOrRefresh(obj => obj.DiscordServerId == commandContext.Guild.Id,
                                           obj =>
                                           {
                                               obj.ApiKey = data.ApiKey;
                                               obj.GuildId = data.GuildId;
                                               obj.DiscordServerId = commandContext.Guild.Id;
                                           });
                }
            }
        }

        /// <summary>
        /// Setting the notification channel
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetNotificationChannel(CommandContextContainer commandContext)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                dbFactory.GetRepository<GuildRepository>()
                         .AddOrRefresh(obj => obj.DiscordServerId == commandContext.Guild.Id,
                                       obj => obj.NotificationChannelId = commandContext.Channel.Id);
            }

            await commandContext.Message.DeleteAsync()
                                .ConfigureAwait(false);
        }

        /// <summary>
        ///  Setting the reminder channel
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetReminderChannel(CommandContextContainer commandContext)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                dbFactory.GetRepository<GuildRepository>()
                         .AddOrRefresh(obj => obj.DiscordServerId == commandContext.Guild.Id,
                                       obj => obj.ReminderChannelId = commandContext.Channel.Id);
            }

            await commandContext.Message.DeleteAsync()
                                .ConfigureAwait(false);
        }

        /// <summary>
        /// Setting up the calendar
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetupMotd(CommandContextContainer commandContext)
        {
            var message = await commandContext.Channel
                                              .SendMessageAsync(LocalizationGroup.GetText("MotdBuilding", "The message of the day will be build with the next refresh."))
                                              .ConfigureAwait(false);

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                if (dbFactory.GetRepository<GuildRepository>()
                             .Refresh(obj => obj.DiscordServerId == commandContext.Guild.Id,
                                      obj =>
                                      {
                                          obj.MessageOfTheDayChannelId = commandContext.Channel.Id;
                                          obj.MessageOfTheDayMessageId = message.Id;
                                      }))
                {
                    await _calendarScheduleService.CreateAppointments(commandContext.Guild.Id)
                                                  .ConfigureAwait(false);

                    await _calendarMessageBuilderService.RefreshMotds(commandContext.Guild.Id)
                                                        .ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Setting up the calendar
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetupCalendar(CommandContextContainer commandContext)
        {
            var data = await DialogHandler.RunForm<SetGuildCalendarFormData>(commandContext, true)
                                          .ConfigureAwait(false);

            if (data != null)
            {
                var message = await commandContext.Channel
                                                  .SendMessageAsync(LocalizationGroup.GetText("CalendarBuilding", "The calendar will be build with the next refresh."))
                                                  .ConfigureAwait(false);

                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    if (dbFactory.GetRepository<GuildRepository>()
                                  .Refresh(obj => obj.DiscordServerId == commandContext.Guild.Id,
                                                obj =>
                                                {
                                                     obj.CalendarDescription = data.Description;
                                                     obj.CalendarTitle = data.Title;
                                                     obj.GuildCalendarChannelId = commandContext.Channel.Id;
                                                     obj.GuildCalendarMessageId = message.Id;
                                                }))
                    {
                        await _calendarScheduleService.CreateAppointments(commandContext.Guild.Id)
                                                      .ConfigureAwait(false);

                        await _calendarMessageBuilderService.RefreshMessages(commandContext.Guild.Id)
                                                            .ConfigureAwait(false);

                        await _calendarMessageBuilderService.RefreshMotds(commandContext.Guild.Id)
                                                            .ConfigureAwait(false);
                    }
                }
            }
        }

        #endregion // Methods
    }
}

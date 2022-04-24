using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Data.Json.Calendar;
using Scruffy.Services.Calendar;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Interfaces;
using Scruffy.Services.Guild.DialogElements.Forms;

namespace Scruffy.Services.Guild;

/// <summary>
/// Configuration of the guild administration
/// </summary>
public class GuildConfigurationService : LocatedServiceBase
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
    public GuildConfigurationService(LocalizationService localizationService, CalendarMessageBuilderService calendarMessageBuilderService, CalendarScheduleService calendarScheduleService)
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
    public async Task CreateGuildConfiguration(IContextContainer commandContext)
    {
        var data = await DialogHandler.RunForm<CreateGuildFormData>(commandContext, true)
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
    /// <param name="type">Type</param>
    public void SetNotificationChannel(IContextContainer commandContext, GuildChannelConfigurationType type)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var guildId = dbFactory.GetRepository<GuildRepository>()
                                   .GetQuery()
                                   .Where(obj => obj.DiscordServerId == commandContext.Guild.Id)
                                   .Select(obj => obj.Id)
                                   .FirstOrDefault();

            if (guildId > 0)
            {
                dbFactory.GetRepository<GuildChannelConfigurationRepository>()
                         .AddOrRefresh(obj => obj.GuildId == guildId
                                           && obj.Type == type,
                                       obj =>
                                       {
                                           obj.GuildId = guildId;
                                           obj.Type = type;
                                           obj.DiscordChannelId = commandContext.Channel.Id;
                                       });
            }
        }
    }

    /// <summary>
    /// Setting up the calendar
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SetupMotd(IContextContainer commandContext)
    {
        var message = await commandContext.Channel
                                          .SendMessageAsync(LocalizationGroup.GetText("MotdBuilding", "The message of the day will be build with the next refresh."))
                                          .ConfigureAwait(false);

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var guildId = dbFactory.GetRepository<GuildRepository>()
                                   .GetQuery()
                                   .Where(obj => obj.DiscordServerId == commandContext.Guild.Id)
                                   .Select(obj => obj.Id)
                                   .FirstOrDefault();

            if (guildId > 0)
            {
                if (dbFactory.GetRepository<GuildChannelConfigurationRepository>()
                             .AddOrRefresh(obj => obj.GuildId == guildId
                                               && obj.Type == GuildChannelConfigurationType.CalendarMessageOfTheDay,
                                           obj =>
                                           {
                                               obj.Type = GuildChannelConfigurationType.CalendarMessageOfTheDay;
                                               obj.DiscordChannelId = commandContext.Channel.Id;
                                               obj.DiscordMessageId = message.Id;
                                           }))
                {
                    await _calendarScheduleService.CreateAppointments(commandContext.Guild.Id)
                                                  .ConfigureAwait(false);

                    await _calendarMessageBuilderService.RefreshMotds(commandContext.Guild.Id)
                                                        .ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Setting up the calendar
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SetupCalendar(IContextContainer commandContext)
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
                var guildId = dbFactory.GetRepository<GuildRepository>()
                                       .GetQuery()
                                       .Where(obj => obj.DiscordServerId == commandContext.Guild.Id)
                                       .Select(obj => obj.Id)
                                       .FirstOrDefault();

                if (guildId > 0)
                {
                    if (dbFactory.GetRepository<GuildChannelConfigurationRepository>()
                                 .AddOrRefresh(obj => obj.GuildId == guildId
                                                   && obj.Type == GuildChannelConfigurationType.CalendarOverview,
                                               obj =>
                                               {
                                                   obj.DiscordChannelId = commandContext.Channel.Id;
                                                   obj.DiscordMessageId = message.Id;
                                                   obj.AdditionalData = JsonConvert.SerializeObject(new AdditionalCalendarChannelData
                                                                                                    {
                                                                                                        Title = data.Title,
                                                                                                        Description = data.Description
                                                                                                    });
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
    }

    #endregion // Methods
}
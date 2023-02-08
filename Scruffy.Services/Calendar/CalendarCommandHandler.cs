using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.Services.Calendar;

/// <summary>
/// Calendar command handler
/// </summary>
public class CalendarCommandHandler : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Participants service
    /// </summary>
    private readonly CalendarParticipantsService _calendarParticipantsService;

    /// <summary>
    /// Templates service
    /// </summary>
    private readonly CalendarTemplateService _calendarTemplateService;

    /// <summary>
    /// Schedules service
    /// </summary>
    private readonly CalendarScheduleService _calendarScheduleService;

    /// <summary>
    /// Calendar message builder service
    /// </summary>
    private readonly CalendarMessageBuilderService _calendarMessageBuilderService;

    /// <summary>
    /// User management service
    /// </summary>
    private readonly UserManagementService _userManagementService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="calendarParticipantsService">Participants service</param>
    /// <param name="calendarScheduleService">Schedules service</param>
    /// <param name="calendarTemplateService">Templates service</param>
    /// <param name="calendarMessageBuilderService">Calendar message builder service</param>
    /// <param name="userManagementService">User management service</param>
    public CalendarCommandHandler(LocalizationService localizationService,
                                  CalendarParticipantsService calendarParticipantsService,
                                  CalendarTemplateService calendarTemplateService,
                                  CalendarScheduleService calendarScheduleService,
                                  CalendarMessageBuilderService calendarMessageBuilderService,
                                  UserManagementService userManagementService)
        : base(localizationService)
    {
        _calendarParticipantsService = calendarParticipantsService;
        _calendarTemplateService = calendarTemplateService;
        _calendarScheduleService = calendarScheduleService;
        _calendarMessageBuilderService = calendarMessageBuilderService;
        _userManagementService = userManagementService;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Set participants
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task SetParticipants(IContextContainer context) => _calendarParticipantsService.EditParticipants(context);

    /// <summary>
    /// Templates configuration
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task ConfigureTemplates(IContextContainer context) => _calendarTemplateService.RunAssistantAsync(context);

    /// <summary>
    /// Schedules configuration
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task ConfigureSchedules(IContextContainer context) => _calendarScheduleService.RunAssistantAsync(context);

    /// <summary>
    /// Add a one time event
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task AddOneTimeEvent(IContextContainer context) => _calendarScheduleService.AddOneTimeEvent(context);

    /// <summary>
    /// Appointment lead configuration
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task LeadConfiguration(InteractionContextContainer context)
    {
        var user = await _userManagementService.GetUserByDiscordAccountId(context.User)
                                               .ConfigureAwait(false);

        var components = context.Interactivity.CreateTemporaryComponentContainer<int>(obj => obj.User.Id == context.User.Id);
        await using (components.ConfigureAwait(false))
        {
            var componentBuilder = new ComponentBuilder();

            var menu = new SelectMenuBuilder().WithCustomId("calendar;lead;selection;")
                                              .WithMinValues(0);

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var now = DateTime.Now;

                foreach (var appointment in dbFactory.GetRepository<CalendarAppointmentRepository>()
                                                     .GetQuery()
                                                     .Where(obj => obj.TimeStamp > now
                                                                && obj.CalendarAppointmentTemplate.DiscordVoiceChannel != null
                                                                && (obj.LeaderId == null
                                                                 || obj.LeaderId == user.Id))
                                                     .Select(obj => new
                                                                    {
                                                                        obj.Id,
                                                                        obj.TimeStamp,
                                                                        obj.CalendarAppointmentTemplate.Description,
                                                                        obj.LeaderId
                                                                    })
                                                     .OrderBy(obj => obj.TimeStamp)
                                                     .Take(10)
                                                     .ToList())
                {
                    menu.AddOption($"{appointment.TimeStamp:g} - {appointment.Description}", appointment.Id.ToString(), null, null, appointment.LeaderId == user.Id);
                }
            }

            menu.MaxValues = menu.Options.Count;

            componentBuilder.WithSelectMenu(menu);

            await context.ReplyAsync(LocalizationGroup.GetText("LeadAppointmentSelection", "Please select all appointments you want to lead."),
                                     components: componentBuilder.Build(),
                                     ephemeral: true)
                         .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Appointment lead selection
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="selection">Selection</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task LeadSelection(InteractionContextContainer context, string[] selection)
    {
        await context.DeleteOriginalResponse()
                     .ConfigureAwait(false);

        var appointmentIds = selection.Select(obj => Convert.ToInt64(obj)).ToList();

        var user = await _userManagementService.GetUserByDiscordAccountId(context.User)
                                               .ConfigureAwait(false);

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var now = DateTime.Now;

            dbFactory.GetRepository<CalendarAppointmentRepository>()
                     .RefreshRange(obj => obj.TimeStamp > now
                                       && obj.LeaderId == user.Id
                                       && appointmentIds.Contains(obj.Id) == false,
                                   obj => obj.LeaderId = null);

            dbFactory.GetRepository<CalendarAppointmentRepository>()
                     .RefreshRange(obj => obj.TimeStamp > now
                                       && obj.LeaderId == null
                                       && appointmentIds.Contains(obj.Id),
                                   obj => obj.LeaderId = user.Id);
        }

        await _calendarMessageBuilderService.RefreshMessages(context.Guild.Id)
                                            .ConfigureAwait(false);
    }

    #endregion // Methods
}
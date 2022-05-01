using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
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

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="calendarParticipantsService">Participants service</param>
    /// <param name="calendarScheduleService">Schedules service</param>
    /// <param name="calendarTemplateService">Templates service</param>
    public CalendarCommandHandler(LocalizationService localizationService,
                                  CalendarParticipantsService calendarParticipantsService,
                                  CalendarTemplateService calendarTemplateService,
                                  CalendarScheduleService calendarScheduleService)
        : base(localizationService)
    {
        _calendarParticipantsService = calendarParticipantsService;
        _calendarTemplateService = calendarTemplateService;
        _calendarScheduleService = calendarScheduleService;
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

    #endregion // Methods
}
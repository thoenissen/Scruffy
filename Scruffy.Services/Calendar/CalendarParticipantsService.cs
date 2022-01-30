using System.Net.Http;

using Scruffy.Data.Services.Calendar;
using Scruffy.Services.Calendar.DialogElements;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar;

/// <summary>
/// Editing the participants of a appointment
/// </summary>
public class CalendarParticipantsService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Localization service
    /// </summary>
    private LocalizationService _localizationService;

    /// <summary>
    /// User management
    /// </summary>
    private UserManagementService _userManagementService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="userManagementService">User management</param>
    /// <param name="httpClientFactory">HttpClient-Factory</param>
    public CalendarParticipantsService(LocalizationService localizationService, UserManagementService userManagementService, IHttpClientFactory httpClientFactory)
        : base(localizationService)
    {
        _localizationService = localizationService;
        _userManagementService = userManagementService;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Editing the participants
    /// </summary>
    /// <param name="commandContext">Command Context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task EditParticipants(CommandContextContainer commandContext)
    {
        var dialogHandler = new DialogHandler(commandContext);
        await using (dialogHandler.ConfigureAwait(false))
        {
            var appointmentId = await dialogHandler.Run<CalendarAppointmentSelectionDialogElement, long>()
                                                   .ConfigureAwait(false);

            bool repeat;

            var participants = new AppointmentParticipantsContainer
                               {
                                   AppointmentId = appointmentId
                               };

            do
            {
                repeat = await dialogHandler.Run<CalendarParticipantsEditDialogElement, bool>(new CalendarParticipantsEditDialogElement(_localizationService, _userManagementService, participants))
                                            .ConfigureAwait(false);
            }
            while (repeat);
        }
    }

    #endregion // Methods
}
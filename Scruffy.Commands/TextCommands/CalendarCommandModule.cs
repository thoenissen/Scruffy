using Discord.Commands;

using Scruffy.Services.Calendar;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Calendar commands
/// </summary>
[Group("calendar")]
[Alias("ca")]
[RequireContext(ContextType.Guild)]
[RequireAdministratorPermissions]
[BlockedChannelCheck]
[HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
public class CalendarCommandModule : LocatedTextCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Calendar schedules service
    /// </summary>
    public CalendarScheduleService CalendarScheduleService { get; set; }

    /// <summary>
    /// Participants service
    /// </summary>
    public CalendarParticipantsService CalendarParticipantsService { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Adding a one time event
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("add")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public async Task AddOneTimeEvent()
    {
        await CalendarScheduleService.AddOneTimeEvent(Context)
                                     .ConfigureAwait(false);
    }

    /// <summary>
    /// Editing participants
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("participants")]
    [Alias("p")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public async Task EditParticipants()
    {
        await CalendarParticipantsService.EditParticipants(Context)
                                         .ConfigureAwait(false);
    }

    #endregion // Methods

    #region Templates

    /// <summary>
    /// Calendar template commands
    /// </summary>
    [Group("templates")]
    [Alias("t")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public class CalendarTemplateCommandModule : LocatedTextCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Calendar template service
        /// </summary>
        public CalendarTemplateService CalendarTemplateService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Starting the template assistant
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public async Task Setup()
        {
            await CalendarTemplateService.RunAssistantAsync(Context)
                                         .ConfigureAwait(false);
        }

        #endregion // Methods

    }

    #endregion // Templates

    #region Schedule

    /// <summary>
    /// Calendar template commands
    /// </summary>
    [Group("schedules")]
    [Alias("s")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public class CalendarScheduleCommandModule : LocatedTextCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Calendar schedules service
        /// </summary>
        public CalendarScheduleService CalendarScheduleService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Starting the schedules assistant
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public async Task Setup()
        {
            await CalendarScheduleService.RunAssistantAsync(Context)
                                         .ConfigureAwait(false);
        }

        #endregion // Methods

    }

    #endregion // Schedule
}
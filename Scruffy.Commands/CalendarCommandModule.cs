using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Services.Calendar;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Commands;

/// <summary>
/// Calendar commands
/// </summary>
[Group("calendar")]
[Aliases("ca")]
[RequireGuild]
[RequireAdministratorPermissions]
[BlockedChannelCheck]
[HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
[ModuleLifespan(ModuleLifespan.Transient)]
public class CalendarCommandModule : LocatedCommandModuleBase
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
    /// <param name="commandContext">Current command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("add")]
    [RequireGuild]
    [RequireAdministratorPermissions]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public Task AddOneTimeEvent(CommandContext commandContext)
    {
        return InvokeAsync(commandContext,
                           async commandContextContainer =>
                           {
                               await CalendarScheduleService.AddOneTimeEvent(commandContextContainer)
                                                            .ConfigureAwait(false);
                           });
    }

    /// <summary>
    /// Editing participants
    /// </summary>
    /// <param name="commandContext">Current command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("participants")]
    [Aliases("p")]
    [RequireGuild]
    [RequireAdministratorPermissions]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public Task EditParticipants(CommandContext commandContext)
    {
        return InvokeAsync(commandContext,
                           async commandContextContainer =>
                           {
                               await CalendarParticipantsService.EditParticipants(commandContextContainer)
                                                                .ConfigureAwait(false);
                           });
    }

    #endregion // Methods

    #region Templates

    /// <summary>
    /// Calendar template commands
    /// </summary>
    [Group("templates")]
    [Aliases("t")]
    [RequireGuild]
    [RequireAdministratorPermissions]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public class CalendarTemplateCommandModule : LocatedCommandModuleBase
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
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public Task Setup(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await CalendarTemplateService.RunAssistantAsync(commandContextContainer)
                                                                .ConfigureAwait(false);
                               });
        }

        #endregion // Methods

    }

    #endregion // Templates

    #region Schedule

    /// <summary>
    /// Calendar template commands
    /// </summary>
    [Group("schedules")]
    [Aliases("s")]
    [RequireGuild]
    [RequireAdministratorPermissions]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
    public class CalendarScheduleCommandModule : LocatedCommandModuleBase
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
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
        public Task Setup(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await CalendarScheduleService.RunAssistantAsync(commandContextContainer)
                                                                .ConfigureAwait(false);
                               });
        }

        #endregion // Methods

    }

    #endregion // Schedule
}
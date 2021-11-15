using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Fractals;

namespace Scruffy.Commands;

/// <summary>
/// Fractal lfg setup commands
/// </summary>
[Group("fractal")]
[Aliases("f")]
[ModuleLifespan(ModuleLifespan.Transient)]
[HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
public class FractalCommandModule : LocatedCommandModuleBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="userManagementService">User management service</param>
    /// <param name="httpClientFactory">HttpClient-Factory</param>
    public FractalCommandModule(LocalizationService localizationService, UserManagementService userManagementService, IHttpClientFactory httpClientFactory)
        : base(localizationService, userManagementService, httpClientFactory)
    {
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// User management service
    /// </summary>
    public UserManagementService UserManagementService { get; set; }

    /// <summary>
    /// Message builder
    /// </summary>
    public FractalLfgMessageBuilder MessageBuilder { get; set; }

    /// <summary>
    /// Fractal reminder service
    /// </summary>
    public FractalLfgReminderService FractalReminderService { get; set; }

    /// <summary>
    /// Fractal registration service
    /// </summary>
    public FractalLfgService LfgService { get; set; }

    #endregion // Properties

    #region Command methods

    /// <summary>
    /// Creation of a new lfg entry
    /// </summary>
    /// <param name="commandContext">Current command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setup")]
    [RequireGuild]
    [RequireAdministratorPermissions]
    public Task Setup(CommandContext commandContext)
    {
        return InvokeAsync(commandContext,
                           commandContextContainer => LfgService.RunSetupAssistant(commandContextContainer));
    }

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="commandContext">Context</param>
    /// <param name="alias">Lfg alias</param>
    /// <param name="timeSpan">Time</param>
    /// <param name="days">Days</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command("join")]
    [RequireGuild]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public Task Join(CommandContext commandContext, string alias, string timeSpan, params string[] days)
    {
        return InvokeAsync(commandContext,
                           commandContextContainer => LfgService.Join(commandContextContainer,
                                                                      new List<string> { alias, timeSpan }.Concat(days)));
    }

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="commandContext">Context</param>
    /// <param name="timeSpan">Time</param>
    /// <param name="days">Days</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command("join")]
    [RequireGuild]
    public Task Join(CommandContext commandContext, string timeSpan, params string[] days)
    {
        return InvokeAsync(commandContext,
                           commandContextContainer => LfgService.Join(commandContextContainer,
                                                                      new List<string> { timeSpan }.Concat(days)));
    }

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="commandContext">Current command context</param>
    /// <param name="alias">Lfg alias</param>
    /// <param name="timeSpan">Time</param>
    /// <param name="days">Days</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("leave")]
    [RequireGuild]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public Task Leave(CommandContext commandContext, string alias, string timeSpan, params string[] days)
    {
        return InvokeAsync(commandContext,
                           commandContextContainer => LfgService.Leave(commandContextContainer,
                                                                       new List<string> { alias, timeSpan }.Concat(days)));
    }

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="commandContext">Current command context</param>
    /// <param name="timeSpan">Time</param>
    /// <param name="days">Days</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("leave")]
    [RequireGuild]
    public Task Leave(CommandContext commandContext, string timeSpan, params string[] days)
    {
        return InvokeAsync(commandContext,
                           commandContextContainer => LfgService.Leave(commandContextContainer,
                                                                       new List<string> { timeSpan }.Concat(days)));
    }

    #endregion // Command methods
}
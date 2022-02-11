using Discord.Commands;

using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;
using Scruffy.Services.Fractals;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Fractal lfg setup commands
/// </summary>
[Group("fractal")]
[Alias("f")]
[BlockedChannelCheck]
[HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
public class FractalCommandModule : LocatedTextCommandModuleBase
{
    #region Properties

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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setup")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task Setup() => LfgService.RunSetupAssistant(Context);

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="alias">Lfg alias</param>
    /// <param name="timeSpan">Time</param>
    /// <param name="days">Days</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command("join")]
    [RequireContext(ContextType.Guild)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public Task Join(string alias, string timeSpan, params string[] days) => LfgService.Join(Context, new List<string> { alias, timeSpan }.Concat(days));

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="timeSpan">Time</param>
    /// <param name="days">Days</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command("join")]
    [RequireContext(ContextType.Guild)]
    public Task Join(string timeSpan, params string[] days) => LfgService.Join(Context, new List<string> { timeSpan }.Concat(days));

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="alias">Lfg alias</param>
    /// <param name="timeSpan">Time</param>
    /// <param name="days">Days</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("leave")]
    [RequireContext(ContextType.Guild)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public Task Leave(string alias, string timeSpan, params string[] days) => LfgService.Leave(Context, new List<string> { alias, timeSpan }.Concat(days));

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="timeSpan">Time</param>
    /// <param name="days">Days</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("leave")]
    [RequireContext(ContextType.Guild)]
    public Task Leave(string timeSpan, params string[] days) => LfgService.Leave(Context, new List<string> { timeSpan }.Concat(days));

    #endregion // Command methods
}
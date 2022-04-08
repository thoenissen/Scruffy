using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.Raid;

namespace Scruffy.Commands.MessageComponents;

/// <summary>
/// Raid component commands
/// </summary>
public class RaidMessageComponentCommandModule : LocatedInteractionModuleBase
{
    #region Constants

    /// <summary>
    /// Group
    /// </summary>
    public const string Group = "raid";

    /// <summary>
    /// Command join
    /// </summary>
    public const string CommandJoin = "join";

    /// <summary>
    /// Command leave
    /// </summary>
    public const string CommandLeave = "leave";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public RaidCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Commands

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="name">Name of the appointment</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ComponentInteraction($"{Group};{CommandJoin};*")]
    public Task Join(string name) => CommandHandler.Join(Context, name);

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="name">Name of the appointment</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ComponentInteraction($"{Group};{CommandLeave};*")]
    public Task Leave(string name) => CommandHandler.Leave(Context, name);

    #endregion // Commands
}
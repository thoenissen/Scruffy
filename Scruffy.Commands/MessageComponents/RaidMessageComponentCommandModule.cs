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
    /// Command role selection
    /// </summary>
    public const string CommandRoleSelection = "roleSelection";

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
    public async Task Join(string name)
    {
        await Context.Interaction
                     .DeferAsync()
                     .ConfigureAwait(false);

        await CommandHandler.Join(Context, name, false)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="name">Name of the appointment</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ComponentInteraction($"{Group};{CommandRoleSelection};*")]
    public async Task RoleSelection(string name)
    {
        await Context.Interaction
                     .DeferAsync()
                     .ConfigureAwait(false);

        await CommandHandler.Join(Context, name, true)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="name">Name of the appointment</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ComponentInteraction($"{Group};{CommandLeave};*")]
    public async Task Leave(string name)
    {
        await Context.Interaction
                     .DeferAsync()
                     .ConfigureAwait(false);

        await CommandHandler.Leave(Context, name)
                            .ConfigureAwait(false);
    }

    #endregion // Commands
}
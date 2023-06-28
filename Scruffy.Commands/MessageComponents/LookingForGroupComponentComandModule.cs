using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.LookingForGroup;

namespace Scruffy.Commands.MessageComponents;

/// <summary>
/// Looking for group commands
/// </summary>
public class LookingForGroupComponentCommandModule : LocatedInteractionModuleBase
{
    #region Constants

    /// <summary>
    /// Group
    /// </summary>
    public const string Group = "lfg";

    /// <summary>
    /// Command create
    /// </summary>
    public const string CommandCreate = "create";

    /// <summary>
    /// Command join
    /// </summary>
    public const string CommandJoin = "join";

    /// <summary>
    /// Command leave
    /// </summary>
    public const string CommandLeave = "leave";

    /// <summary>
    /// Command configuration
    /// </summary>
    public const string CommandConfiguration = "configuration";

    /// <summary>
    /// Configure menu options
    /// </summary>
    public const string CommandConfigureMenu = "configureMenu";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public LookingForGroupCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Commands

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="appointmentId">Id of the appointment</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ComponentInteraction($"{Group};{CommandJoin};*")]
    public async Task Join(int appointmentId)
    {
        await Context.DeferAsync()
                     .ConfigureAwait(false);

        await CommandHandler.Join(Context, appointmentId)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="appointmentId">Id of the appointment</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ComponentInteraction($"{Group};{CommandLeave};*")]
    public async Task Leave(int appointmentId)
    {
        await Context.DeferAsync()
                     .ConfigureAwait(false);

        await CommandHandler.Leave(Context, appointmentId)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="appointmentId">Id of the appointment</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ComponentInteraction($"{Group};{CommandConfiguration};*")]
    public async Task Configure(int appointmentId)
    {
        await Context.DeferAsync()
                     .ConfigureAwait(false);

        await CommandHandler.Configure(Context, appointmentId)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Configure menu options
    /// </summary>
    /// <param name="appointmentId">Id of the configuration</param>
    /// <param name="value">Value</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ComponentInteraction($"{Group};{CommandConfigureMenu};*")]
    public async Task SelectRoles(int appointmentId, string value)
    {
        await CommandHandler.ConfigureMenuOption(Context, appointmentId, value)
                            .ConfigureAwait(false);
    }

    #endregion // Commands
}
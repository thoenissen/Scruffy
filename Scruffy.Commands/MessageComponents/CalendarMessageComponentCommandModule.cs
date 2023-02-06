using Discord.Interactions;

using Scruffy.Services.Calendar;
using Scruffy.Services.Discord;

namespace Scruffy.Commands.MessageComponents;

/// <summary>
/// Calendar component commands
/// </summary>
public class CalendarMessageComponentCommandModule : LocatedInteractionModuleBase
{
    #region Constants

    /// <summary>
    /// Group
    /// </summary>
    public const string Group = "calendar";

    /// <summary>
    /// Command lead configuration
    /// </summary>
    public const string CommandLeadConfiguration = "lead;configuration";

    /// <summary>
    /// Command lead selection
    /// </summary>
    public const string CommandLeadSelection = "lead;selection";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public CalendarCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Commands

    /// <summary>
    /// Appointment lead configuration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ComponentInteraction($"{Group};{CommandLeadConfiguration};")]
    public async Task LeadConfiguration()
    {
        await Context.DeferAsync()
                     .ConfigureAwait(false);

        await CommandHandler.LeadConfiguration(Context)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Appointment lead selection
    /// </summary>
    /// <param name="selection">Selection</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ComponentInteraction($"{Group};{CommandLeadSelection};")]
    public async Task LeadSelection(string[] selection)
    {
        await Context.DeferAsync()
                     .ConfigureAwait(false);

        await CommandHandler.LeadSelection(Context, selection)
                            .ConfigureAwait(false);
    }

    #endregion // Commands
}
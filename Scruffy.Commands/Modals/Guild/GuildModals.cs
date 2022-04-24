using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.Guild;
using Scruffy.Services.Guild.Modals;

namespace Scruffy.Commands.Modals.Guild;

/// <summary>
/// Guild modals
/// </summary>
public class GuildModals : LocatedInteractionModuleBase
{
    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public GuildCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Export stash
    /// </summary>
    /// <param name="modal">Modal input</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [ModalInteraction(GuildExportStashModalData.CustomId)]
    public Task ExportStash(GuildExportStashModalData modal) => CommandHandler.ExportStash(Context, modal.Mode, modal.SinceDate, modal.SinceTime);

    /// <summary>
    /// Export upgrades
    /// </summary>
    /// <param name="modal">Modal input</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [ModalInteraction(GuildExportUpgradesModalData.CustomId)]
    public Task ExportUpgrades(GuildExportUpgradesModalData modal) => CommandHandler.ExportUpgrades(Context, modal.Mode, modal.SinceDate, modal.SinceTime);

    /// <summary>
    /// Export current points
    /// </summary>
    /// <param name="modal">Modal input</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [ModalInteraction(GuildExportCurrentPointsModalData.CustomId)]
    public Task ExportCurrentPoints(GuildExportCurrentPointsModalData modal) => CommandHandler.ExportCurrentPoints(Context, modal.SinceDate);

    #endregion // Methods
}
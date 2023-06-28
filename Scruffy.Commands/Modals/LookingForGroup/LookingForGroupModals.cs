using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.LookingForGroup;
using Scruffy.Services.LookingForGroup.Modals;

namespace Scruffy.Commands.Modals.LookingForGroup;

/// <summary>
/// Guild modals
/// </summary>
public class LookingForGroupModals : LocatedInteractionModuleBase
{
    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public LookingForGroupCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Edit
    /// </summary>
    /// <param name="appointmentId">Appointment id</param>
    /// <param name="modal">Modal input</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [ModalInteraction($"{LookingForGroupEditModalData.CustomId};*")]
    public Task Edit(int appointmentId, LookingForGroupEditModalData modal) => CommandHandler.Edit(Context, appointmentId, modal.AppointmentTitle, modal.AppointmentDescription);

    #endregion // Methods
}
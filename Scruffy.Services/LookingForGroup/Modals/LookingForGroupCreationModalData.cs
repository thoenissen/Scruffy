using Discord;
using Discord.Interactions;

namespace Scruffy.Services.LookingForGroup.Modals
{
    /// <summary>
    /// Appointment creation
    /// </summary>
    public class LookingForGroupCreationModalData : IModal
    {
        /// <summary>
        /// Custom id
        /// </summary>
        public const string CustomId = "modal;lfg;creation";

        /// <summary>
        /// Creation of the appointment
        /// </summary>
        public string Title => "Appointment creation";

        /// <summary>
        /// Title
        /// </summary>
        [InputLabel("Title")]
        [RequiredInput]
        [ModalTextInput(nameof(AppointmentTitle))]
        public string AppointmentTitle { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [InputLabel("Description")]
        [RequiredInput(false)]
        [ModalTextInput(nameof(AppointmentDescription), TextInputStyle.Paragraph)]
        public string AppointmentDescription { get; set; }
    }
}
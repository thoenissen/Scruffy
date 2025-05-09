using Discord;
using Discord.Interactions;

namespace Scruffy.Services.LookingForGroup.Modals;

/// <summary>
/// Appointment edit
/// </summary>
public class LookingForGroupEditModalData : IModal
{
    /// <summary>
    /// Custom id
    /// </summary>
    public const string CustomId = "modal;lfg;edit";

    /// <summary>
    /// Creation of the appointment
    /// </summary>
    public string Title => "Appointment creation";

    /// <summary>
    /// Title
    /// </summary>
    [InputLabel("Title")]
    [RequiredInput]
    [ModalTextInput(nameof(AppointmentTitle), maxLength: 95)]
    public string AppointmentTitle { get; set; }

    /// <summary>
    /// Time
    /// </summary>
    [InputLabel("Time")]
    [RequiredInput(false)]
    [ModalTextInput(nameof(AppointmentTime), TextInputStyle.Short, "dd.mm.yyyy hh:mm", 16, 16)]
    public string AppointmentTime { get; set; }

    /// <summary>
    /// Number of participants
    /// </summary>
    [InputLabel("Participants")]
    [RequiredInput(false)]
    [ModalTextInput(nameof(AppointmentParticipantCount))]
    public string AppointmentParticipantCount { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    [InputLabel("Description")]
    [RequiredInput(false)]
    [ModalTextInput(nameof(AppointmentDescription), TextInputStyle.Paragraph)]
    public string AppointmentDescription { get; set; }
}
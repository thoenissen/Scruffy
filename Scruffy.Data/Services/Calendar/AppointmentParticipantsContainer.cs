using System.Collections.Generic;

namespace Scruffy.Data.Services.Calendar;

/// <summary>
/// Participants data
/// </summary>
public class AppointmentParticipantsContainer
{
    /// <summary>
    /// Id of the appointment
    /// </summary>
    public long AppointmentId { get; set; }

    /// <summary>
    /// Participants
    /// </summary>
    public List<CalendarAppointmentParticipantData> Participants { get; set; }
}
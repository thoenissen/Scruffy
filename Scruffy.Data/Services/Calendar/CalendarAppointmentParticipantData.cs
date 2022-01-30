using Discord;

namespace Scruffy.Data.Services.Calendar;

/// <summary>
/// Participant
/// </summary>
public class CalendarAppointmentParticipantData
{
    /// <summary>
    /// Member
    /// </summary>
    public IGuildUser Member { get; set; }

    /// <summary>
    /// Leader
    /// </summary>
    public bool IsLeader { get; set; }
}
namespace Scruffy.WebApp.DTOs.Calendar;

/// <summary>
/// Participant data for a calendar appointment
/// </summary>
public class CalendarParticipantDTO
{
    /// <summary>
    /// Internal user ID
    /// </summary>
    public long UserId { get; init; }

    /// <summary>
    /// Discord account ID
    /// </summary>
    public ulong DiscordAccountId { get; init; }

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Is the participant a leader?
    /// </summary>
    public bool IsLeader { get; set; }
}
namespace Scruffy.WebApp.DTOs.Raid;

/// <summary>
/// Participation status of a user in a raid appointment
/// </summary>
public enum RaidParticipationStatus
{
    /// <summary>
    /// The user participated in the raid
    /// </summary>
    Played,

    /// <summary>
    /// The user was on the substitute bench
    /// </summary>
    Substitute,

    /// <summary>
    /// The user did not show up and receives penalty points
    /// </summary>
    NoShow
}
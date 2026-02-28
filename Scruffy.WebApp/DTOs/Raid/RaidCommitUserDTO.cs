namespace Scruffy.WebApp.DTOs.Raid;

/// <summary>
/// User data for raid commit
/// </summary>
public class RaidCommitUserDTO
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
    /// Base participation points from the experience level
    /// </summary>
    public double ParticipationPoints { get; init; }

    /// <summary>
    /// Participation status (played, substitute, no-show, or late registration)
    /// </summary>
    public RaidParticipationStatus Status { get; set; }

    /// <summary>
    /// Calculated points based on the participation status
    /// </summary>
    public double Points
    {
        get
        {
            return Status switch
            {
                RaidParticipationStatus.Played => ParticipationPoints * 1.0,
                RaidParticipationStatus.Substitute => ParticipationPoints * 3.0,
                RaidParticipationStatus.NoShow => ParticipationPoints * -1.0,
                RaidParticipationStatus.LateRegistration => 0.0,
                _ => ParticipationPoints * 1.0,
            };
        }
    }

    /// <summary>
    /// Experience level description
    /// </summary>
    public string ExperienceLevelDescription { get; init; }
}
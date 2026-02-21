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
    /// Is the user on the substitutes bench?
    /// </summary>
    public bool IsSubstitute { get; set; }

    /// <summary>
    /// Calculated points based on the participation status
    /// </summary>
    public double Points => IsSubstitute ? ParticipationPoints * 3.0 : ParticipationPoints * 1.0;

    /// <summary>
    /// Experience level description
    /// </summary>
    public string ExperienceLevelDescription { get; init; }
}
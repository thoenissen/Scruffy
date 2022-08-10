namespace Scruffy.ServiceHosts.WebApi.DTO.Raid;

/// <summary>
/// Raid participant
/// </summary>
public class RaidParticipantDTO
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Roles of the user
    /// </summary>
    public List<long> Roles { get; set; }

    /// <summary>
    /// Preferred roles
    /// </summary>
    public List<long> PreferredRoles { get; set; }
}
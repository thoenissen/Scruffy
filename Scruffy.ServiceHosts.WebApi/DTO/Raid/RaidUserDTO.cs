namespace Scruffy.ServiceHosts.WebApi.DTO.Raid;

/// <summary>
/// Raid user
/// </summary>
public class RaidUserDTO
{
    /// <summary>
    /// ID of the user
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Name of the user
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Roles which are assigned to the user
    /// </summary>
    public List<long> AssignedRoles { get; set; }

    /// <summary>
    /// Special roles which are assigned to the user
    /// </summary>
    public List<long> AssignedSpecialRoles { get; set; }
}
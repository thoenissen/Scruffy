namespace Scruffy.WebApp.DTOs.Administration;

/// <summary>
/// User
/// </summary>
public class UserDTO
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
    /// Guild Wars 2 Account name
    /// </summary>
    public string GuildWarsAccountName { get; set; }
}
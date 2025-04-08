namespace Scruffy.WebApp.DTOs.Administration;

/// <summary>
/// User
/// </summary>
public class UserDTO
{
    /// <summary>
    /// Discord account name
    /// </summary>
    public string DiscordAccountName { get; set; }

    /// <summary>
    /// Guild Wars 2 Account name
    /// </summary>
    public string GuildWarsAccountName { get; set; }

    /// <summary>
    /// Is the user a member of the guild?
    /// </summary>
    public bool IsGuildMember { get; set; }

    /// <summary>
    /// Is the api key valid?
    /// </summary>
    public bool IsApiKeyValid { get; set; }
}
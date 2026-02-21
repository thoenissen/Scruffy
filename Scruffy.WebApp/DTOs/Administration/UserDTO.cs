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

    /// <summary>
    /// Excluded from ranking changes?
    /// </summary>
    public bool IsFixedRank { get; set; }

    /// <summary>
    /// Inactive user?
    /// </summary>
    public bool IsInactive { get; set; }

    /// <summary>
    /// Internal user id
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// Internal guild id
    /// </summary>
    public long? GuildId { get; set; }

    /// <summary>
    /// Guild Wars 2 API key
    /// </summary>
    public string GuildWarsAccountApiKey { get; set; }
}
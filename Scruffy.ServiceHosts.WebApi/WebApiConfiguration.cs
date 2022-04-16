namespace Scruffy.ServiceHosts.WebApi;

/// <summary>
/// Web API configuration
/// </summary>
public class WebApiConfiguration
{
    /// <summary>
    /// Discord server id of the corresponding guild
    /// </summary>
    public static ulong DiscordServerId { get; } = Convert.ToUInt64(Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_SERVER_ID"));
}
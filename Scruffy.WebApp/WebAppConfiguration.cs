using System;

namespace Scruffy.WebApp;

/// <summary>
/// Web application configuration
/// </summary>
public static class WebAppConfiguration
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    static WebAppConfiguration()
    {
        if (ulong.TryParse(Environment.GetEnvironmentVariable("SCRUFFY_GUILD_SERVER"), out var discordServerId))
        {
            DiscordServerId = discordServerId;
        }
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// ID of the discord server
    /// </summary>
    public static ulong DiscordServerId { get; }

    #endregion // Properties
}
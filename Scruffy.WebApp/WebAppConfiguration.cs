using System;

namespace Scruffy.WebApp;

/// <summary>
/// Web application configuration
/// </summary>
public static class WebAppConfiguration
{
    #region Nested classes

    /// <summary>
    /// Colors
    /// </summary>
    public static class Colors
    {
        /// <summary>
        /// Text
        /// </summary>
        public const string Text = "#eae9fc";

        /// <summary>
        /// Background
        /// </summary>
        public const string Background = "#171a1c";

        /// <summary>
        /// Primary
        /// </summary>
        public const string Primary = "#351d3f";

        /// <summary>
        /// Secondary
        /// </summary>
        public const string Secondary = "#2F3147";

        /// <summary>
        /// Ascent
        /// </summary>
        public const string Accent = "#f1d083";
    }

    #endregion // Nested classes

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
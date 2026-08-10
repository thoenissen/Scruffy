using System;

using Scruffy.Services.Core;

namespace Scruffy.WebApp;

/// <summary>
/// Web application configuration
/// </summary>
public static class WebAppConfiguration
{
    #region Fields

    /// <summary>
    /// ID of the discord server
    /// </summary>
    private static ulong? _discordServerId;

    #endregion // Fields

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

    #region Properties

    /// <summary>
    /// ID of the discord server
    /// </summary>
    public static ulong DiscordServerId
    {
        get
        {
            if (_discordServerId == null)
            {
                if (ulong.TryParse(ConfigurationService.GetEntry("SCRUFFY_GUILD_SERVER"), out var discordServerId) == false)
                {
                    _discordServerId = discordServerId;
                }
                else
                {
                    _discordServerId = 0;
                }
            }

            return _discordServerId.Value;
        }
    }

    #endregion // Properties
}
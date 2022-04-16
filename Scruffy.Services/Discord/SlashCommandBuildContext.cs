using System.Globalization;

using Discord;

namespace Scruffy.Services.Discord;

/// <summary>
/// Slash command build context
/// </summary>
public class SlashCommandBuildContext
{
    /// <summary>
    /// Guild
    /// </summary>
    public IGuild Guild { get; set; }

    /// <summary>
    /// Service provider
    /// </summary>
    public IServiceProvider ServiceProvider { get; set; }

    /// <summary>
    /// Culture information
    /// </summary>
    public CultureInfo CultureInfo { get; set; }
}
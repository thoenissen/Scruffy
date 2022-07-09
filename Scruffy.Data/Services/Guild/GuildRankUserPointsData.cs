using Scruffy.Data.Enumerations.Guild;

namespace Scruffy.Data.Services.Guild;

/// <summary>
/// User points
/// </summary>
public class GuildRankUserPointsData
{
    /// <summary>
    /// Type
    /// </summary>
    public GuildRankPointType Type { get; set; }

    /// <summary>
    /// Points
    /// </summary>
    public double Points { get; set; }
}
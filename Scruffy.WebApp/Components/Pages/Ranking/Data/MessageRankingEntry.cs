namespace Scruffy.WebApp.Components.Pages.Ranking.Data;

/// <summary>
/// Entry for the message-based user ranking
/// </summary>
internal record MessageRankingEntry
{
    /// <summary>
    /// User name
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Total message count
    /// </summary>
    public int MessageCount { get; init; }

    /// <summary>
    /// Calculated level based on message count
    /// </summary>
    public int Level { get; init; }

    /// <summary>
    /// Progress towards the next level (0–100)
    /// </summary>
    public double LevelProgressPercent { get; init; }
}
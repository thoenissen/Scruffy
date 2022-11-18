using Newtonsoft.Json;

namespace Scruffy.Data.Json.DpsReport;

/// <summary>
/// Target
/// </summary>
public class Target
{
    /// <summary>
    /// Game ID of the target
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// Final health of the target
    /// </summary>
    [JsonProperty("finalHealth")]
    public int FinalHealth { get; set; }

    /// <summary>
    /// % of health burned
    /// </summary>
    [JsonProperty("healthPercentBurned")]
    public double HealthPercentBurned { get; set; }

    /// <summary>
    /// Time at which target became active
    /// </summary>
    [JsonProperty("firstAware")]
    public int FirstAware { get; set; }

    /// <summary>
    /// Time at which target became inactive
    /// </summary>
    [JsonProperty("lastAware")]
    public int LastAware { get; set; }

    /// <summary>
    /// Indicates that the Target is actually an enemy player
    /// </summary>
    [JsonProperty("enemyPlayer")]
    public bool EnemyPlayer { get; set; }

    /// <summary>
    /// Array of double[2] that represents the breakbar percent of the actor
    /// Value[i][0] will be the time, value[i][1] will be breakbar %
    /// If i corresponds to the last element that means the breakbar did not change for the remainder of the fight
    /// </summary>
    [JsonProperty("breakbarPercents")]
    public IReadOnlyList<IReadOnlyList<double>> BreakbarPercents { get; set; }

    /// <summary>
    /// Indicates that the JsonActor does not exist in reality
    /// </summary>
    [JsonProperty("isFake")]
    public bool IsFake { get; set; }
}
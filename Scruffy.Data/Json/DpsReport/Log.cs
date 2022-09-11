using System.Globalization;

using Newtonsoft.Json;

namespace Scruffy.Data.Json.DpsReport;

/// <summary>
/// Log
/// </summary>
public class Log
{
    /// <summary>
    /// GW2 build
    /// </summary>
    [JsonProperty("gW2Build")]
    public ulong GW2Build { get; set; }

    /// <summary>
    /// The time at which the fight started in "yyyy-mm-dd hh:mm:ss zzz" format
    /// </summary>
    [JsonProperty("timeStartStd")]
    public DateTimeOffset TimeStart { get; set; }

    /// <summary>
    /// The time at which the fight ended in "yyyy-mm-dd hh:mm:ss zzz" format
    /// </summary>
    [JsonProperty("timeEndStd")]
    public DateTimeOffset TimeEnd { get; set; }

    /// <summary>
    /// The total duration of the fight
    /// </summary>
    public TimeSpan Duration => TimeEnd - TimeStart;

    /// <summary>
    /// The success status of the fight
    /// </summary>
    [JsonProperty("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Whether the fight is in challenge mode
    /// </summary>
    [JsonProperty("isCM")]
    public bool IsCM { get; set; }

    /// <summary>
    /// The list of targets
    /// </summary>
    [JsonProperty("targets")]
    public IReadOnlyList<Target> Targets { get; set; }

    /// <summary>
    /// The remaining total health of all target combined
    /// </summary>
    public double? RemainingTotalHealth
    {
        get
        {
            var remainingHealth = 0.0;
            var validTargets = 0;

            foreach (var target in Targets)
            {
                // Ignore fake targets & CC targets in Aetherblade Hideout & Hearts in Dragonvoid & Dhuum Reaper
                if (!target.IsFake && !target.EnemyPlayer && target.Id != 23656 && target.Id != -23 && target.Id != 19831)
                {
                    remainingHealth += 100.0 - target.HealthPercentBurned;

                    // The last 10% of Captain Mai Trin never gets removed
                    if (target.Id == 24033 && remainingHealth < 10.0)
                    {
                        remainingHealth = 0;
                    }

                    ++validTargets;
                }
            }

            return validTargets > 0 ? remainingHealth / validTargets : null;
        }
    }
}
using System.Globalization;

using Newtonsoft.Json;

namespace Scruffy.Data.Json.DpsReport;

/// <summary>
/// Log
/// </summary>
public class Log
{
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
                // Ignore fake targets & CC targets in Aetherblade Hideout & Hearts in Dragonvoid
                if (!target.IsFake && !target.EnemyPlayer && target.Id != 23656 && target.Id != -23)
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
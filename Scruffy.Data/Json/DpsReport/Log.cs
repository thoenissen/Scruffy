using System.Globalization;

using Newtonsoft.Json;

namespace Scruffy.Data.Json.DpsReport;

/// <summary>
/// Log
/// </summary>
public class Log
{
    /// <summary>
    /// The time at which the fight started in "yyyy-MM-dd HH:mm:ss zzz" format
    /// The value will be DefaultTimeValue if the event does not exist
    /// </summary>
    [JsonProperty("timeStartStd")]
    public string TimeStartStd { get; set; }

    /// <summary>
    /// The time at which the fight ended in "yyyy-MM-dd HH:mm:ss zzz" format
    /// The value will be DefaultTimeValue if the event does not exist
    /// </summary>
    [JsonProperty("timeEndStd")]
    public string TimeEndStd { get; set; }

    /// <summary>
    /// The list of targets
    /// </summary>
    [JsonProperty("targets")]
    public IReadOnlyList<Target> Targets { get; set; }

    /// <summary>
    /// The duration of the fight
    /// </summary>
    public TimeSpan? Duration
    {
        get
        {
            return DateTime.TryParseExact(TimeStartStd, "yyyy-MM-dd HH:mm:ss zzz", null, DateTimeStyles.None, out var start)
                && DateTime.TryParseExact(TimeEndStd, "yyyy-MM-dd HH:mm:ss zzz", null, DateTimeStyles.None, out var end)
                ? end - start
                : null;
        }
    }

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
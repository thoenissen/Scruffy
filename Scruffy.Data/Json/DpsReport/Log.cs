using System.Globalization;

using Newtonsoft.Json;

namespace Scruffy.Data.Json.DpsReport;

/// <summary>
/// Log
/// </summary>
public class Log
{
    /// <summary>
    /// Fight name
    /// </summary>
    [JsonProperty("fightName")]
    public string FightName { get; set; }

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
            var totalHealth = 0.0;
            var hasValidTargets = false;

            foreach (var target in Targets)
            {
                // Fake targets
                if (target.IsFake == false

                 && target.EnemyPlayer == false

                 // CC targets
                 // Aetherblade Hideout
                 && target.Id != 23656

                 // Hearts in Dragonvoid
                 && target.Id != -23

                 // Dhuum Reaper
                 && target.Id != 19831)
                {
                    totalHealth += target.TotalHealth;

                    // The last 10% of Captain Mai Trin never gets removed
                    if (target.Id != 24033
                     || target.HealthPercentBurned < 90.0)
                    {
                        remainingHealth += target.FinalHealth;
                    }

                    hasValidTargets = true;
                }
            }

            return hasValidTargets ? remainingHealth / totalHealth * 100 : null;
        }
    }
}
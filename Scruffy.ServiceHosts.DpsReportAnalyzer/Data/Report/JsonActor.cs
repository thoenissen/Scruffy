namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Base class for Players and NPCs
    /// </summary>
    /// <seealso cref="JsonPlayer"/>
    /// <seealso cref="JsonNpc"/>
    public abstract class JsonActor
    {
        /// <summary>
        /// Name of the actor
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Total health of the actor. -1 if information is missing (ex: players)
        /// </summary>
        public int TotalHealth { get; set; }

        /// <summary>
        /// Condition damage score
        /// </summary>
        public ulong Condition { get; set; }

        /// <summary>
        /// Concentration score
        /// </summary>
        public ulong Concentration { get; set; }

        /// <summary>
        /// Healing Power score
        /// </summary>
        public ulong Healing { get; set; }

        /// <summary>
        /// Toughness score
        /// </summary>
        public ulong Toughness { get; set; }

        /// <summary>
        /// Height of the hitbox
        /// </summary>
        public ulong HitboxHeight { get; set; }

        /// <summary>
        /// Width of the hitbox
        /// </summary>
        public ulong HitboxWidth { get; set; }

        /// <summary>
        /// ID of the actor in the instance
        /// </summary>
        public ushort InstanceId { get; set; }

        /// <summary>
        /// List of minions
        /// </summary>
        /// <seealso cref="JsonMinions"/>
        public IReadOnlyList<JsonMinions> Minions { get; set; }

        /// <summary>
        /// Indicates that the JsonActor does not exist in reality
        /// </summary>
        public bool IsFake { get; set; }

        /// <summary>
        /// Array of Total DPS stats
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="JsonDps"/>
        public IReadOnlyList<JsonDps> DpsAll { get; set; }

        /// <summary>
        /// Stats against all
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="JsonGameplayStatsAll"/>
        public IReadOnlyList<JsonGameplayStatsAll> StatsAll { get; set; }

        /// <summary>
        /// Defensive stats
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="JsonDefensesAll"/>
        public IReadOnlyList<JsonDefensesAll> Defenses { get; set; }

        /// <summary>
        /// Total Damage distribution array
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="JsonDamageDist"/>
        public IReadOnlyList<IReadOnlyList<JsonDamageDist>> TotalDamageDist { get; set; }

        /// <summary>
        /// Damage taken array
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="JsonDamageDist"/>
        public IReadOnlyList<IReadOnlyList<JsonDamageDist>> TotalDamageTaken { get; set; }

        /// <summary>
        /// Rotation data
        /// </summary>
        /// <seealso cref="JsonRotation"/>
        public IReadOnlyList<JsonRotation> Rotation { get; set; }

        // TODO /// <summary>
        // TODO /// Array of int representing 1S damage points
        // TODO /// Length == # of phases
        // TODO /// </summary>
        // TODO /// <remarks>
        // TODO /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        // TODO /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        // TODO /// </remarks>
        // TODO public IReadOnlyList<IReadOnlyList<int>> Damage1S { get; set; }
        // TODO
        // TODO /// <summary>
        // TODO /// Array of int representing 1S power damage points
        // TODO /// Length == # of phases
        // TODO /// </summary>
        // TODO /// <remarks>
        // TODO /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        // TODO /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        // TODO /// </remarks>
        // TODO public IReadOnlyList<IReadOnlyList<int>> PowerDamage1S { get; set; }
        // TODO
        // TODO /// <summary>
        // TODO /// Array of int representing 1S condition damage points
        // TODO /// Length == # of phases
        // TODO /// </summary>
        // TODO /// <remarks>
        // TODO /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        // TODO /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        // TODO /// </remarks>
        // TODO public IReadOnlyList<IReadOnlyList<int>> ConditionDamage1S { get; set; }
        // TODO
        // TODO /// <summary>
        // TODO /// Array of double representing 1S breakbar damage points
        // TODO /// Length == # of phases
        // TODO /// </summary>
        // TODO /// <remarks>
        // TODO /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        // TODO /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        // TODO /// </remarks>
        // TODO public IReadOnlyList<IReadOnlyList<double>> BreakbarDamage1S { get; set; }

        /// <summary>
        /// Array of int[2] that represents the number of conditions
        /// Array[i][0] will be the time, Array[i][1] will be the number of conditions present from Array[i][0] to Array[i+1][0]
        /// If i corresponds to the last element that means the status did not change for the remainder of the fight
        /// </summary>
        public IReadOnlyList<IReadOnlyList<int>> ConditionsStates { get; set; }

        /// <summary>
        /// Array of int[2] that represents the number of boons
        /// Array[i][0] will be the time, Array[i][1] will be the number of boons present from Array[i][0] to Array[i+1][0]
        /// If i corresponds to the last element that means the status did not change for the remainder of the fight
        /// </summary>
        public IReadOnlyList<IReadOnlyList<int>> BoonsStates { get; set; }

        /// <summary>
        /// Array of int[2] that represents the number of active combat minions
        /// Array[i][0] will be the time, Array[i][1] will be the number of active combat minions present from Array[i][0] to Array[i+1][0]
        /// If i corresponds to the last element that means the status did not change for the remainder of the fight
        /// </summary>
        public IReadOnlyList<IReadOnlyList<int>> ActiveCombatMinions { get; set; }

        /// <summary>
        /// Array of double[2] that represents the health status of the actor
        /// Array[i][0] will be the time, Array[i][1] will be health %
        /// If i corresponds to the last element that means the health did not change for the remainder of the fight
        /// </summary>
        public IReadOnlyList<IReadOnlyList<double>> HealthPercents { get; set; }

        /// <summary>
        /// Array of double[2] that represents the barrier status of the actor
        /// Array[i][0] will be the time, Array[i][1] will be barrier %
        /// If i corresponds to the last element that means the health did not change for the remainder of the fight
        /// </summary>
        public IReadOnlyList<IReadOnlyList<double>> BarrierPercents { get; set; }

        // TODO /// <summary>
        // TODO /// Contains combat replay related data
        // TODO /// </summary>
        // TODO /// <seealso cref="JsonActorCombatReplayData"/>
        // TODO public JsonActorCombatReplayData CombatReplayData { get; set; }
    }
}
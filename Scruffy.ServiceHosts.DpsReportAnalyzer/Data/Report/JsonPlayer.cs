using MongoDB.Bson.Serialization.Attributes;

namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Class representing a player
    /// </summary>
    public class JsonPlayer : JsonActor
    {
        /// <summary>
        /// Account name of the player
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// Group of the player
        /// </summary>
        public int Group { get; set; }

        /// <summary>
        /// Indicates if a player has a commander tag
        /// </summary>
        public bool HasCommanderTag { get; set; }

        /// <summary>
        /// Profession of the player
        /// </summary>
        public string Profession { get; set; }

        /// <summary>
        /// Indicates that the JsonPlayer is actually a friendly NPC
        /// </summary>
        public bool FriendlyNpc { get; set; }

        /// <summary>
        /// Indicates that the JsonPlayer is not actually part of the squad
        /// </summary>
        public bool NotInSquad { get; set; }

        /// <summary>
        /// Guild id to be used to fetch from the official API
        /// </summary>
        public string GuildId { get; set; }

        /// <summary>
        /// Weapons of the player
        /// 0-1 are the first land set, 1-2 are the second land set
        /// 3-4 are the first aquatic set, 5-6 are the second aquatic set
        /// When unknown, 'Unknown' value will appear
        /// If 2 handed weapon even indices will have "2Hand" as value
        /// </summary>
        public IReadOnlyList<string> Weapons { get; set; }

        /// <summary>
        /// Array of int[2] that represents the number of active clones
        /// Array[i][0] will be the time, Array[i][1] will be the number of clones present from Array[i][0] to Array[i+1][0]
        /// If i corresponds to the last element that means the status did not change for the remainder of the fight
        /// Only relevant for clone summoning capable specs.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<int>> ActiveClones { get; set; }

        /// <summary>
        /// Array of Total DPS stats
        /// Length == # of targets and the length of each sub array is equal to # of phases
        /// </summary>
        /// <seealso cref="JsonDps"/>
        public IReadOnlyList<IReadOnlyList<JsonDps>> DpsTargets { get; set; }

        /// <summary>
        /// Array of int representing 1S damage points
        /// Length == # of targets and the length of each sub array is equal to # of phases
        /// </summary>
        /// <remarks>
        /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        /// </remarks>
        [BsonIgnore]
        public IReadOnlyList<IReadOnlyList<IReadOnlyList<int>>> TargetDamage1S { get; set; }

        /// <summary>
        /// Array of int representing 1S power damage points
        /// Length == # of targets and the length of each sub array is equal to # of phases
        /// </summary>
        /// <remarks>
        /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        /// </remarks>
        [BsonIgnore]
        public IReadOnlyList<IReadOnlyList<IReadOnlyList<int>>> TargetPowerDamage1S { get; set; }

        /// <summary>
        /// Array of int representing 1S condition damage points
        /// Length == # of targets and the length of each sub array is equal to # of phases
        /// </summary>
        /// <remarks>
        /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        /// </remarks>
        [BsonIgnore]
        public IReadOnlyList<IReadOnlyList<IReadOnlyList<int>>> TargetConditionDamage1S { get; set; }

        /// <summary>
        /// Array of double representing 1S breakbar damage points
        /// Length == # of targets and the length of each sub array is equal to # of phases
        /// </summary>
        /// <remarks>
        /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        /// </remarks>
        [BsonIgnore]
        public IReadOnlyList<IReadOnlyList<IReadOnlyList<double>>> TargetBreakbarDamage1S { get; set; }

        /// <summary>
        /// Per Target Damage distribution array
        /// Length == # of targets and the length of each sub array is equal to # of phases
        /// </summary>
        /// <seealso cref="JsonDamageDist"/>
        [BsonIgnore]
        public IReadOnlyList<IReadOnlyList<IReadOnlyList<JsonDamageDist>>> TargetDamageDist { get; set; }

        /// <summary>
        /// Stats against targets
        /// Length == # of targets and the length of each sub array is equal to # of phases
        /// </summary>
        /// <seealso cref="JsonGameplayStats"/>
        public IReadOnlyList<IReadOnlyList<JsonGameplayStats>> StatsTargets { get; set; }

        /// <summary>
        /// Support stats
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="JsonPlayerSupport"/>
        public IReadOnlyList<JsonPlayerSupport> Support { get; set; }

        /// <summary>
        /// Damage modifiers against all
        /// </summary>
        /// <seealso cref="JsonDamageModifierData"/>
        [BsonIgnore]
        public IReadOnlyList<JsonDamageModifierData> DamageModifiers { get; set; }

        /// <summary>
        /// Damage modifiers against targets
        /// Length == # of targets
        /// </summary>
        /// <seealso cref="JsonDamageModifierData"/>
        [BsonIgnore]
        public IReadOnlyList<IReadOnlyList<JsonDamageModifierData>> DamageModifiersTarget { get; set; }

        /// <summary>
        /// List of buff status
        /// </summary>
        /// <seealso cref="JsonBuffsUptime"/>
        public IReadOnlyList<JsonBuffsUptime> BuffUptimes { get; set; }

        /// <summary>
        /// List of buff status on self generation
        /// Key is "'b' + id"
        /// </summary>
        /// <seealso cref="JsonBuffsUptime"/>
        public IReadOnlyList<JsonPlayerBuffsGeneration> SelfBuffs { get; set; }

        /// <summary>
        /// List of buff status on group generation
        /// </summary>
        /// <seealso cref="JsonPlayerBuffsGeneration"/>
        public IReadOnlyList<JsonPlayerBuffsGeneration> GroupBuffs { get; set; }

        /// <summary>
        /// List of buff status on off group generation
        /// </summary>
        /// <seealso cref="JsonPlayerBuffsGeneration"/>
        public IReadOnlyList<JsonPlayerBuffsGeneration> OffGroupBuffs { get; set; }

        /// <summary>
        /// List of buff status on squad generation
        /// </summary>
        /// <seealso cref="JsonPlayerBuffsGeneration"/>
        public IReadOnlyList<JsonPlayerBuffsGeneration> SquadBuffs { get; set; }

        /// <summary>
        /// List of buff status on active time
        /// </summary>
        /// <seealso cref="JsonBuffsUptime"/>
        [BsonIgnore]
        public IReadOnlyList<JsonBuffsUptime> BuffUptimesActive { get; set; }

        /// <summary>
        /// List of buff status on self generation on active time
        /// </summary>
        /// <seealso cref="JsonBuffsUptime"/>
        [BsonIgnore]
        public IReadOnlyList<JsonPlayerBuffsGeneration> SelfBuffsActive { get; set; }

        /// <summary>
        /// List of buff status on group generation on active time
        /// <seealso cref="JsonPlayerBuffsGeneration"/>
        /// </summary>
        [BsonIgnore]
        public IReadOnlyList<JsonPlayerBuffsGeneration> GroupBuffsActive { get; set; }

        /// <summary>
        /// List of buff status on off group generation on active time
        /// <seealso cref="JsonPlayerBuffsGeneration"/>
        /// </summary>
        [BsonIgnore]
        public IReadOnlyList<JsonPlayerBuffsGeneration> OffGroupBuffsActive { get; set; }

        /// <summary>
        /// List of buff status on squad generation on active time
        /// </summary>
        /// <seealso cref="JsonPlayerBuffsGeneration"/>
        [BsonIgnore]
        public IReadOnlyList<JsonPlayerBuffsGeneration> SquadBuffsActive { get; set; }

        /// <summary>
        /// List of death recaps
        /// Length == number of death
        /// </summary>
        /// <seealso cref="JsonDeathRecap"/>
        public IReadOnlyList<JsonDeathRecap> DeathRecap { get; set; }

        /// <summary>
        /// List of used consumables
        /// </summary>
        /// <seealso cref="JsonConsumable"/>
        public IReadOnlyList<JsonConsumable> Consumables { get; set; }

        /// <summary>
        /// List of time during which the player was active (not dead and not dc)
        /// Length == number of phases
        /// </summary>
        public IReadOnlyList<long> ActiveTimes { get; set; }

        /// <summary>
        /// Healing stats data
        /// </summary>
        [BsonIgnore]
        public ExtJsonPlayerHealingStats ExtHealingStats { get; set; }

        /// <summary>
        /// Barrier stats data
        /// </summary>
        [BsonIgnore]
        public ExtJsonPlayerBarrierStats ExtBarrierStats { get; set; }
    }
}
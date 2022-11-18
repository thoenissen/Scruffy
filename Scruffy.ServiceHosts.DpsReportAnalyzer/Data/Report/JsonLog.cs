namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// The root of the JSON
    /// </summary>
    public class JsonLog
    {
        /// <summary>
        /// The used EI version.
        /// </summary>
        public string EliteInsightsVersion { get; set; }

        /// <summary>
        /// The id with which the log has been triggered
        /// </summary>
        public int TriggerId { get; set; }

        /// <summary>
        /// The elite insight id of the log, indicates which encounter the log corresponds to.
        /// see https://github.com/baaron4/GW2-Elite-Insights-Parser/blob/master/EncounterIDs.md/
        /// </summary>
        public long EiEncounterId { get; set; }

        /// <summary>
        /// The name of the fight
        /// </summary>
        public string FightName { get; set; }

        /// <summary>
        /// The icon of the fight
        /// </summary>
        public string FightIcon { get; set; }

        /// <summary>
        /// The used arcdps version
        /// </summary>
        public string ArcVersion { get; set; }

        /// <summary>
        /// GW2 build
        /// </summary>
        public ulong Gw2Build { get; set; }

        /// <summary>
        /// Language with which the evtc was generated
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// ID of the language
        /// </summary>
        public byte LanguageId { get; set; }

        /// <summary>
        /// The player who recorded the fight
        /// </summary>
        public string RecordedBy { get; set; }

        /// <summary>
        /// DEPRECATED: use TimeStartStd instead
        /// The time at which the fight started in "yyyy-mm-dd hh:mm:ss zz" format
        /// </summary>
        public string TimeStart { get; set; }

        /// <summary>
        /// DEPRECATED: use TimeEndStd instead
        /// The time at which the fight ended in "yyyy-mm-dd hh:mm:ss zz" format
        /// </summary>
        public string TimeEnd { get; set; }

        /// <summary>
        /// The time at which the fight started in "yyyy-mm-dd hh:mm:ss zzz" format
        /// </summary>
        public string TimeStartStd { get; set; }

        /// <summary>
        /// The time at which the fight ended in "yyyy-mm-dd hh:mm:ss zzz" format
        /// </summary>
        public string TimeEndStd { get; set; }

        /// <summary>
        /// The duration of the fight in "xh xm xs xms" format
        /// </summary>
        public string Duration { get; set; }

        /// <summary>
        /// The duration of the fight in ms
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// Offset between fight start and log start
        /// </summary>
        public long LogStartOffset { get; set; }

        /// <summary>
        /// The success status of the fight
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// If the fight is in challenge mode
        /// </summary>
        public bool IsCm { get; set; }

        /// <summary>
        /// The list of targets
        /// </summary>
        /// <seealso cref="JsonNpc"/>
        public IReadOnlyList<JsonNpc> Targets { get; set; }

        /// <summary>
        /// The list of players
        /// </summary>
        /// <seealso cref="JsonPlayer"/>
        public IReadOnlyList<JsonPlayer> Players { get; set; }

        /// <summary>
        /// The list of phases
        /// </summary>
        /// <seealso cref="JsonPhase"/>
        public IReadOnlyList<JsonPhase> Phases { get; set; }

        /// <summary>
        /// List of mechanics
        /// </summary>
        /// <seealso cref="JsonMechanics"/>
        public IReadOnlyList<JsonMechanics> Mechanics { get; set; }

        /// <summary>
        /// Upload links to dps.reports/raidar
        /// </summary>
        public IReadOnlyList<string> UploadLinks { get; set; }

        /// <summary>
        /// Dictionary of skills' description, the key is in "'s' + id" format
        /// </summary>
        /// <seealso cref="SkillDesc"/>
        public IReadOnlyDictionary<string, SkillDesc> SkillMap { get; set; }

        /// <summary>
        /// Dictionary of buffs' description, the key is in "'b' + id" format
        /// </summary>
        /// <seealso cref="BuffDesc"/>
        public IReadOnlyDictionary<string, BuffDesc> BuffMap { get; set; }

        /// <summary>
        /// Dictionary of damage modifiers' description, the key is in "'d' + id" format
        /// </summary>
        /// <seealso cref="DamageModDesc"/>
        public IReadOnlyDictionary<string, DamageModDesc> DamageModMap { get; set; }

        /// <summary>
        /// Dictionary of personal buffs. The key is the profession, the value is a list of buff ids
        /// </summary>
        /// <seealso cref="BuffMap"/>
        public IReadOnlyDictionary<string, IReadOnlyCollection<long>> PersonalBuffs { get; set; }

        /// <summary>
        /// List of present fractal instabilities, the values are buff ids. DEPRECATED: use PresentInstanceBuffs instead
        /// </summary>
        /// <seealso cref="BuffMap"/>
        public IReadOnlyList<long> PresentFractalInstabilities { get; set; }

        /// <summary>
        /// List of present instance buffs, values are arrays of 2 elements, value[0] is buff id, value[1] is number of stacks.
        /// </summary>
        /// <seealso cref="BuffMap"/>
        public IReadOnlyList<long[]> PresentInstanceBuffs { get; set; }

        /// <summary>
        /// List of error messages given by ArcDPS
        /// </summary>
        public IReadOnlyList<string> LogErrors { get; set; }

        /// <summary>
        /// List of used extensions
        /// </summary>
        public IReadOnlyList<ExtensionDesc> UsedExtensions { get; set; }

        // TODO /// <summary>
        // TODO /// Contains combat replay related meta data
        // TODO /// </summary>
        // TODO /// <seealso cref="JsonCombatReplayMetaData"/>
        // TODO public JsonCombatReplayMetaData CombatReplayMetaData { get; set; }
    }
}
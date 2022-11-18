namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Class representing buffs generation by player actors
    /// </summary>
    public class JsonPlayerBuffsGeneration
    {
        /// <summary>
        /// ID of the buff
        /// </summary>
        /// <seealso cref="JsonLog.BuffMap"/>
        public long Id { get; set; }

        /// <summary>
        /// Array of buff data
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="JsonBuffsGenerationData"/>
        public IReadOnlyList<JsonBuffsGenerationData> BuffData { get; set; }
    }
}
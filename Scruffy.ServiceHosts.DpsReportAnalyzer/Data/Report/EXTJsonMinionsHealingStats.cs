namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Class regrouping minion related healing statistics
    /// </summary>
    public class ExtJsonMinionsHealingStats
    {
        /// <summary>
        /// Total Healing done by minions
        /// Length == # of phases
        /// </summary>
        public IReadOnlyList<int> TotalHealing { get; set; }

        /// <summary>
        /// Total Allied Healing done by minions
        /// Length == # of players and the length of each sub array is equal to # of phases
        /// </summary>
        public IReadOnlyList<IReadOnlyList<int>> TotalAlliedHealing { get; set; }

        /// <summary>
        /// Total Outgoing Healing distribution array
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="ExtJsonHealingDist"/>
        public IReadOnlyList<IReadOnlyList<ExtJsonHealingDist>> TotalHealingDist { get; set; }

        /// <summary>
        /// Total Outgoing Allied Healing distribution array
        /// Length == # of players and the length of each sub array is equal to # of phases
        /// </summary>
        /// <seealso cref="ExtJsonHealingDist"/>
        public IReadOnlyList<IReadOnlyList<IReadOnlyList<ExtJsonHealingDist>>> AlliedHealingDist { get; set; }
    }
}
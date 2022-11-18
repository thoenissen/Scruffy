namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Class regrouping minion related Barrier statistics
    /// </summary>
    public class ExtJsonMinionsBarrierStats
    {
        /// <summary>
        /// Total barrier done by minions
        /// Length == # of phases
        /// </summary>
        public IReadOnlyList<int> TotalBarrier { get; set; }

        /// <summary>
        /// Total Allied Barrier done by minions
        /// Length == # of players and the length of each sub array is equal to # of phases
        /// </summary>
        public IReadOnlyList<IReadOnlyList<int>> TotalAlliedBarrier { get; set; }

        /// <summary>
        /// Total Outgoing Barrier distribution array
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="ExtJsonBarrierDist"/>
        public IReadOnlyList<IReadOnlyList<ExtJsonBarrierDist>> TotalBarrierDist { get; set; }

        /// <summary>
        /// Total Outgoing Allied Barrier distribution array
        /// Length == # of players and the length of each sub array is equal to # of phases
        /// </summary>
        /// <seealso cref="ExtJsonBarrierDist"/>
        public IReadOnlyList<IReadOnlyList<IReadOnlyList<ExtJsonBarrierDist>>> AlliedBarrierDist { get; set; }
    }
}
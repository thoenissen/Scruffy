namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Class regrouping player related barrier statistics
    /// </summary>
    public class ExtJsonPlayerBarrierStats
    {
        /// <summary>
        /// Array of Total Allied Outgoing Barrier stats
        /// Length == # of players and the length of each sub array is equal to # of phases
        /// </summary>
        /// <seealso cref="EXTJsonOutgoingBarrierStatistics"/>
        public IReadOnlyList<IReadOnlyList<EXTJsonOutgoingBarrierStatistics>> OutgoingBarrierAllies { get; set; }

        /// <summary>
        /// Array of Total Outgoing Barrier stats
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="EXTJsonOutgoingBarrierStatistics"/>
        public IReadOnlyList<EXTJsonOutgoingBarrierStatistics> OutgoingBarrier { get; set; }

        /// <summary>
        /// Array of Total Incoming Barrier stats
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="EXTJsonOutgoingBarrierStatistics"/>
        public IReadOnlyList<EXTJsonOutgoingBarrierStatistics> IncomingBarrier { get; set; }

        /// <summary>
        /// Array of int representing 1S outgoing allied barrier points
        /// Length == # of players and the length of each sub array is equal to # of phases
        /// </summary>
        /// <remarks>
        /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        /// </remarks>
        public IReadOnlyList<IReadOnlyList<IReadOnlyList<int>>> AlliedBarrier1S { get; set; }

        /// <summary>
        /// Array of int representing 1S outgoing barrier points
        /// Length == # of phases
        /// </summary>
        /// <remarks>
        /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        /// </remarks>
        public IReadOnlyList<IReadOnlyList<int>> Barrier1S { get; set; }

        /// <summary>
        /// Total Outgoing Allied Barrier distribution array
        /// Length == # of players and the length of each sub array is equal to # of phases
        /// </summary>
        /// <seealso cref="ExtJsonBarrierDist"/>
        public IReadOnlyList<IReadOnlyList<IReadOnlyList<ExtJsonBarrierDist>>> AlliedBarrierDist { get; set; }

        /// <summary>
        /// Total Outgoing Barrier distribution array
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="ExtJsonBarrierDist"/>
        public IReadOnlyList<IReadOnlyList<ExtJsonBarrierDist>> TotalBarrierDist { get; set; }

        /// <summary>
        /// Total Incoming Barrier distribution array
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="ExtJsonBarrierDist"/>
        public IReadOnlyList<IReadOnlyList<ExtJsonBarrierDist>> TotalIncomingBarrierDist { get; set; }
    }
}
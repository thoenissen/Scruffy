namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Outgoing barrier statistics
    /// </summary>
    public class EXTJsonOutgoingBarrierStatistics
    {
        /// <summary>
        /// Total bps
        /// </summary>
        public int Bps { get; set; }

        /// <summary>
        /// Total barrier
        /// </summary>
        public int Barrier { get; set; }

        /// <summary>
        /// Total actor only Bps
        /// </summary>
        public int ActorBps { get; set; }

        /// <summary>
        /// Total actor only barrier
        /// </summary>
        public int ActorBarrier { get; set; }
    }
}
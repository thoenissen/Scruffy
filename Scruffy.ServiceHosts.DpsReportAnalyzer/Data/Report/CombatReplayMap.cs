namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Represents a combat replay map
    /// </summary>
    public class CombatReplayMap
    {
        /// <summary>
        /// Url of the map
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Interval in ms, between which the map should be used.\n
        /// Interval[0] is start and Interval[1] is end.
        /// </summary>
        public long[] Interval { get; set; }
    }
}
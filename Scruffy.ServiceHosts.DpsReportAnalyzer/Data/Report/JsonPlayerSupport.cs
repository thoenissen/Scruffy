namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Support stats
    /// </summary>
    public class JsonPlayerSupport
    {
        /// <summary>
        /// Number of time ressurected someone
        /// </summary>
        public long Resurrects { get; set; }

        /// <summary>
        /// Time passed on ressurecting
        /// </summary>
        public double ResurrectTime { get; set; }

        /// <summary>
        /// Number of time a condition was removed, self excluded
        /// </summary>
        public long CondiCleanse { get; set; }

        /// <summary>
        /// Total time of condition removed, self excluded
        /// </summary>
        public double CondiCleanseTime { get; set; }

        /// <summary>
        /// Number of time a condition was removed from self
        /// </summary>
        public long CondiCleanseSelf { get; set; }

        /// <summary>
        /// Total time of condition removed from self
        /// </summary>
        public double CondiCleanseTimeSelf { get; set; }

        /// <summary>
        /// Number of time a boon was removed
        /// </summary>
        public long BoonStrips { get; set; }

        /// <summary>
        /// Total time of boons removed from self
        /// </summary>
        public double BoonStripsTime { get; set; }
    }
}
namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Class representing buff on targets
    /// </summary>
    public class JsonBuffsUptime
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
        /// <seealso cref="JsonBuffsUptimeData"/>
        public IReadOnlyList<JsonBuffsUptimeData> BuffData { get; set; }

        /// <summary>
        /// Array of int[2] that represents the number of buff
        /// Array[i][0] will be the time, Array[i][1] will be the number of buff present from Array[i][0] to Array[i+1][0]
        /// If i corresponds to the last element that means the status did not change for the remainder of the fight
        /// </summary>
        public IReadOnlyList<IReadOnlyList<int>> States { get; set; }
    }
}
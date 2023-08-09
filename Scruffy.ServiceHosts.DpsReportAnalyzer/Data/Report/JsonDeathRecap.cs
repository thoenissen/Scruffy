namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Class corresponding to a death recap
    /// </summary>
    public class JsonDeathRecap
    {
        /// <summary>
        /// Time of death
        /// </summary>
        public long DeathTime { get; set; }

        /// <summary>
        /// List of damaging events to put into downstate
        /// </summary>
        public IReadOnlyList<JsonDeathRecapDamageItem> ToDown { get; set; }

        /// <summary>
        /// List of damaging events to put into deadstate
        /// </summary>
        public IReadOnlyList<JsonDeathRecapDamageItem> ToKill { get; set; }
    }
}
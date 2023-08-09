namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Incoming healing statistics
    /// </summary>
    public class ExtJsonIncomingHealingStatistics
    {
        /// <summary>
        /// Total received healing
        /// </summary>
        public int Healed { get; set; }

        /// <summary>
        /// Total received healing power based healing
        /// </summary>
        public int HealingPowerHealed { get; set; }

        /// <summary>
        /// Total received conversion based healing
        /// </summary>
        public int ConversionHealed { get; set; }

        /// <summary>
        /// Total received hybrid healing
        /// </summary>
        public int HybridHealed { get; set; }

        /// <summary>
        /// Total healing received while downed
        /// </summary>
        public int DownedHealed { get; set; }
    }
}
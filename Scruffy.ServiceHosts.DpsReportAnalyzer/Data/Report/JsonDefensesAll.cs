namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Defensive stats
    /// </summary>
    public class JsonDefensesAll
    {
        /// <summary>
        /// Total damage taken
        /// </summary>
        public long DamageTaken { get; set; }

        /// <summary>
        /// Total breakbar damage taken
        /// </summary>
        public double BreakbarDamageTaken { get; set; }

        /// <summary>
        /// Number of blocks
        /// </summary>
        public int BlockedCount { get; set; }

        /// <summary>
        /// Number of evades
        /// </summary>
        public int EvadedCount { get; set; }

        /// <summary>
        /// Number of misses
        /// </summary>
        public int MissedCount { get; set; }

        /// <summary>
        /// Number of dodges
        /// </summary>
        public int DodgeCount { get; set; }

        /// <summary>
        /// Number of time an incoming attack was negated by invul
        /// </summary>
        public int InvulnedCount { get; set; }

        /// <summary>
        /// Damage done against barrier
        /// </summary>
        public int DamageBarrier { get; set; }

        /// <summary>
        /// Number of time interrupted
        /// </summary>
        public int InterruptedCount { get; set; }

        /// <summary>
        /// Number of time downed
        /// </summary>
        public int DownCount { get; set; }

        /// <summary>
        /// Time passed in downstate
        /// </summary>
        public long DownDuration { get; set; }

        /// <summary>
        /// Number of time died
        /// </summary>
        public int DeadCount { get; set; }

        /// <summary>
        /// Time passed in dead state
        /// </summary>
        public long DeadDuration { get; set; }

        /// <summary>
        /// Number of time disconnected
        /// </summary>
        public int DcCount { get; set; }

        /// <summary>
        /// Time passed in disconnected state
        /// </summary>
        public long DcDuration { get; set; }
    }
}
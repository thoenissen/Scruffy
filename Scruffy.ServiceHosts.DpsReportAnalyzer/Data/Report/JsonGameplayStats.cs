namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Gameplay stats
    /// </summary>
    public class JsonGameplayStats
    {
        /// <summary>
        /// Number of damage hit
        /// </summary>
        public int TotalDamageCount { get; set; }

        /// <summary>
        /// Number of direct damage hit
        /// </summary>
        public int DirectDamageCount { get; set; }

        /// <summary>
        /// Number of connected direct damage hit
        /// </summary>
        public int ConnectedDirectDamageCount { get; set; }

        /// <summary>
        /// Number of connected damage hit
        /// </summary>
        public int ConnectedDamageCount { get; set; }

        /// <summary>
        /// Number of critable hit
        /// </summary>
        public int CritableDirectDamageCount { get; set; }

        /// <summary>
        /// Number of crit
        /// </summary>
        public int CriticalRate { get; set; }

        /// <summary>
        /// Total critical damage
        /// </summary>
        public int CriticalDmg { get; set; }

        /// <summary>
        /// Number of hits while flanking
        /// </summary>
        public int FlankingRate { get; set; }

        /// <summary>
        /// Number of hits while target was moving
        /// </summary>
        public int AgainstMovingRate { get; set; }

        /// <summary>
        /// Number of glanced hits
        /// </summary>
        public int GlanceRate { get; set; }

        /// <summary>
        /// Number of missed hits
        /// </summary>
        public int Missed { get; set; }

        /// <summary>
        /// Number of evaded hits
        /// </summary>
        public int Evaded { get; set; }

        /// <summary>
        /// Number of blocked hits
        /// </summary>
        public int Blocked { get; set; }

        /// <summary>
        /// Number of hits that interrupted a skill
        /// </summary>
        public int Interrupts { get; set; }

        /// <summary>
        /// Number of hits against invulnerable targets
        /// </summary>
        public int Invulned { get; set; }

        /// <summary>
        /// Number of times killed target
        /// </summary>
        public int Killed { get; set; }

        /// <summary>
        /// Number of times downed target
        /// </summary>
        public int Downed { get; set; }
    }
}
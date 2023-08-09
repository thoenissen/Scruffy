namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Gameplay stats
    /// </summary>
    public class JsonGameplayStatsAll : JsonGameplayStats
    {
        /// <summary>
        /// Number of time you interrupted your cast
        /// </summary>
        public int Wasted { get; set; }

        /// <summary>
        /// Time wasted by interrupting your cast
        /// </summary>
        public double TimeWasted { get; set; }

        /// <summary>
        /// Number of time you skipped an aftercast
        /// </summary>
        public int Saved { get; set; }

        /// <summary>
        /// Time saved while skipping aftercast
        /// </summary>
        public double TimeSaved { get; set; }

        /// <summary>
        /// Distance to the epicenter of the squad
        /// </summary>
        public double StackDist { get; set; }

        /// <summary>
        /// Distance to the commander of the squad. Only when a player with commander tag is present
        /// </summary>
        public double DistToCom { get; set; }

        /// <summary>
        /// Average amount of boons
        /// </summary>
        public double AvgBoons { get; set; }

        /// <summary>
        /// Average amount of boons over active time
        /// </summary>
        public double AvgActiveBoons { get; set; }

        /// <summary>
        /// Average amount of conditions
        /// </summary>
        public double AvgConditions { get; set; }

        /// <summary>
        /// Average amount of conditions over active time
        /// </summary>
        public double AvgActiveConditions { get; set; }

        /// <summary>
        /// Number of time a weapon swap happened
        /// </summary>
        public int SwapCount { get; set; }

        /// <summary>
        /// % of time in combat spent in animation
        /// </summary>
        public double SkillCastUptime { get; set; }

        /// <summary>
        /// % of time in combat spent in animation, excluding auto attack skills
        /// </summary>
        public double SkillCastUptimeNoAa { get; set; }
    }
}
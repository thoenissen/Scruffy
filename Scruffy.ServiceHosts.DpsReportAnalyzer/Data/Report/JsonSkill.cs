namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Class corresponding to a skill
    /// </summary>
    public class JsonSkill
    {
        /// <summary>
        /// Time at which the skill was cast
        /// </summary>
        public int CastTime { get; set; }

        /// <summary>
        /// Duration of the animation, instant cast if 0
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Gained time from the animation, could be negative, which means time was lost
        /// </summary>
        public int TimeGained { get; set; }

        /// <summary>
        /// Value between -1 (100% slow) and 1 (100% quickness)
        /// Prior arcdps activation update (nov 07 2019) this value can only be 0 or 1
        /// </summary>
        public double Quickness { get; set; }
    }
}
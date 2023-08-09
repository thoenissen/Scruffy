namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Describes the skill item
    /// </summary>
    public class SkillDesc
    {
        /// <summary>
        /// Name of the skill
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// If the skill is an auto attack
        /// </summary>
        public bool AutoAttack { get; set; }

        /// <summary>
        /// If the skill can crit
        /// </summary>
        public bool CanCrit { get; set; }

        /// <summary>
        /// Icon of the skill
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// True if this skill can trigger on weapon swap sigils
        /// </summary>
        public bool IsSwap { get; set; }

        /// <summary>
        /// True when the skill is an instant cast
        /// </summary>
        public bool IsInstantCast { get; set; }

        /// <summary>
        /// True when the skill is an instant cast and the detection may have missed some
        /// </summary>
        public bool IsNotAccurate { get; set; }

        /// <summary>
        /// If the skill is encountered in a healing context, true if healing happened because of conversion, false otherwise
        /// </summary>
        public bool ConversionBasedHealing { get; set; }

        /// <summary>
        /// If the skill is encountered in a healing context, true if healing could have happened due to conversion or healing power
        /// </summary>
        public bool HybridHealing { get; set; }
    }
}
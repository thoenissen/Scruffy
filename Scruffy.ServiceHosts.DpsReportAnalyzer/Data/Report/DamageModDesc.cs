namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Describs the damage modifier item
    /// </summary>
    public class DamageModDesc
    {
        /// <summary>
        /// Name of the damage modifier
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Icon of the damage modifier
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Description of the damage modifier
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// False if the modifier is multiplicative
        /// If true then the correspond <see cref="JsonDamageModifierItem.DamageGain"/> are damage done under the effect. One will have to deduce the gain manualy depending on your gear.
        /// </summary>
        public bool NonMultiplier { get; set; }

        /// <summary>
        /// True if the modifier is skill based
        /// </summary>
        public bool SkillBased { get; set; }

        /// <summary>
        /// True if the modifier is an approximation
        /// </summary>
        public bool Approximate { get; set; }
    }
}
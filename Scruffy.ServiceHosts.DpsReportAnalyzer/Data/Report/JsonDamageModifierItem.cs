namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Class corresponding to a buff based damage modifier
    /// </summary>
    public class JsonDamageModifierItem
    {
        /// <summary>
        /// Hits done under the buff
        /// </summary>
        public int HitCount { get; set; }

        /// <summary>
        /// Total hits
        /// </summary>
        public int TotalHitCount { get; set; }

        /// <summary>
        /// Gained damage
        /// If the corresponding <see cref="DamageModDesc.NonMultiplier"/> is true then this value correspond to the damage done while under the effect. One will have to deduce the gain manualy depending on your gear.
        /// </summary>
        public double DamageGain { get; set; }

        /// <summary>
        /// Total damage done
        /// </summary>
        public int TotalDamage { get; set; }
    }
}
namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Class representing damage modifier data
    /// </summary>
    public class JsonDamageModifierData
    {
        /// <summary>
        /// ID of the damage modifier \
        /// </summary>
        /// <seealso cref="JsonLog.DamageModMap"/>
        public int Id { get; set; }

        /// <summary>
        /// Array of damage modifier data
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="JsonDamageModifierItem"/>
        public IReadOnlyList<JsonDamageModifierItem> DamageModifiers { get; set; }
    }
}
namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// DPS stats
    /// </summary>
    public class JsonDps
    {
        /// <summary>
        /// Total dps
        /// </summary>
        public int Dps { get; set; }

        /// <summary>
        /// Total damage
        /// </summary>
        public int Damage { get; set; }

        /// <summary>
        /// Total condi dps
        /// </summary>
        public int CondiDps { get; set; }

        /// <summary>
        /// Total condi damage
        /// </summary>
        public int CondiDamage { get; set; }

        /// <summary>
        /// Total power dps
        /// </summary>
        public int PowerDps { get; set; }

        /// <summary>
        /// Total power damage
        /// </summary>
        public int PowerDamage { get; set; }

        /// <summary>
        /// Total breakbar damage
        /// </summary>
        public double BreakbarDamage { get; set; }

        /// <summary>
        /// Total actor only dps
        /// </summary>
        public int ActorDps { get; set; }

        /// <summary>
        /// Total actor only damage
        /// </summary>
        public int ActorDamage { get; set; }

        /// <summary>
        /// Total actor only condi dps
        /// </summary>
        public int ActorCondiDps { get; set; }

        /// <summary>
        /// Total actor only condi damage
        /// </summary>
        public int ActorCondiDamage { get; set; }

        /// <summary>
        /// Total actor only power dps
        /// </summary>
        public int ActorPowerDps { get; set; }

        /// <summary>
        /// Total actor only power damage
        /// </summary>
        public int ActorPowerDamage { get; set; }

        /// <summary>
        /// Total actor only breakbar damage
        /// </summary>
        public double ActorBreakbarDamage { get; set; }
    }
}
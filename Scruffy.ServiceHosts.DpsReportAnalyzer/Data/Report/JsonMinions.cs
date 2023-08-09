namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Class corresponding to the regrouping of the same type of minions
    /// </summary>
    public class JsonMinions
    {
        /// <summary>
        /// Name of the minion
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Game ID of the minion
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Total Damage done by minions
        /// Length == # of phases
        /// </summary>
        public IReadOnlyList<int> TotalDamage { get; set; }

        /// <summary>
        /// Damage done by minions against targets
        /// Length == # of targets and the length of each sub array is equal to # of phases
        /// </summary>
        public IReadOnlyList<IReadOnlyList<int>> TotalTargetDamage { get; set; }

        /// <summary>
        /// Total Breakbar Damage done by minions
        /// Length == # of phases
        /// </summary>
        public IReadOnlyList<double> TotalBreakbarDamage { get; set; }

        /// <summary>
        /// Breakbar Damage done by minions against targets
        /// Length == # of targets and the length of each sub array is equal to # of phases
        /// </summary>
        public IReadOnlyList<IReadOnlyList<double>> TotalTargetBreakbarDamage { get; set; }

        /// <summary>
        /// Total Shield Damage done by minions
        /// Length == # of phases
        /// </summary>
        public IReadOnlyList<int> TotalShieldDamage { get; set; }

        /// <summary>
        /// Shield Damage done by minions against targets
        /// Length == # of targets and the length of each sub array is equal to # of phases
        /// </summary>
        public IReadOnlyList<IReadOnlyList<int>> TotalTargetShieldDamage { get; set; }

        /// <summary>
        /// Total Damage distribution array
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="JsonDamageDist"/>
        public IReadOnlyList<IReadOnlyList<JsonDamageDist>> TotalDamageDist { get; set; }

        /// <summary>
        /// Per Target Damage distribution array
        /// Length == # of targets and the length of each sub array is equal to # of phases
        /// </summary>
        /// <seealso cref="JsonDamageDist"/>
        public IReadOnlyList<IReadOnlyList<IReadOnlyList<JsonDamageDist>>> TargetDamageDist { get; set; }

        /// <summary>
        /// Rotation data
        /// </summary>
        /// <seealso cref="JsonRotation"/>
        public IReadOnlyList<JsonRotation> Rotation { get; set; }

        // TODO /// <summary>
        // TODO /// Healing stats data
        // TODO /// </summary>
        // TODO public ExtJsonMinionsHealingStats ExtHealingStats { get; set; }
        // TODO
        // TODO /// <summary>
        // TODO /// Barrier stats data
        // TODO /// </summary>
        // TODO public ExtJsonMinionsBarrierStats ExtBarrierStats { get; set; }
    }
}
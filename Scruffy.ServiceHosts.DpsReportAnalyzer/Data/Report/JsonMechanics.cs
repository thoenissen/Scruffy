namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Class corresponding to mechanics
    /// </summary>
    public class JsonMechanics
    {
        /// <summary>
        /// List of mechanics application
        /// </summary>
        public IReadOnlyList<JsonMechanic> MechanicsData { get; set; }

        /// <summary>
        /// Name of the mechanic
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the mechanic
        /// </summary>
        public string Description { get; set; }
    }
}
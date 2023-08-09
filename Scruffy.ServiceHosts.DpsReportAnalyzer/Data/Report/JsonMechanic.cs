namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Class corresponding to a mechanic event
    /// </summary>
    public class JsonMechanic
    {
        /// <summary>
        /// Time at which the event happened
        /// </summary>
        public long Time { get; set; }

        /// <summary>
        /// The actor who is concerned by the mechanic
        /// </summary>
        public string Actor { get; set; }
    }
}
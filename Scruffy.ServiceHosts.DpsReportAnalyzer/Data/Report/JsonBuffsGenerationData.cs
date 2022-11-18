namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Player buffs generation item
    /// </summary>
    public class JsonBuffsGenerationData
    {
        /// <summary>
        /// Generation done
        /// </summary>
        public double Generation { get; set; }

        /// <summary>
        /// Generation with overstack
        /// </summary>
        public double Overstack { get; set; }

        /// <summary>
        /// Wasted generation
        /// </summary>
        public double Wasted { get; set; }

        /// <summary>
        /// Extension from unknown source
        /// </summary>
        public double UnknownExtended { get; set; }

        /// <summary>
        /// Generation done by extension
        /// </summary>
        public double ByExtension { get; set; }

        /// <summary>
        /// Buff extended
        /// </summary>
        public double Extended { get; set; }
    }
}
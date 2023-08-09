namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Describs the buff item
    /// </summary>
    public class BuffDesc
    {
        /// <summary>
        /// Name of the buff
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Icon of the buff
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// True if the buff is stacking
        /// </summary>
        public bool Stacking { get; set; }

        /// <summary>
        /// If the buff is encountered in a healing context, true if healing happened because of conversion, false otherwise
        /// </summary>
        public bool ConversionBasedHealing { get; set; }

        /// <summary>
        /// If the buff is encountered in a healing context, true if healing could have happened due to conversion or healing power
        /// </summary>
        public bool HybridHealing { get; set; }

        /// <summary>
        /// Descriptions of the buffs (no traits)
        /// </summary>
        public IReadOnlyList<string> Descriptions { get; set; }
    }
}
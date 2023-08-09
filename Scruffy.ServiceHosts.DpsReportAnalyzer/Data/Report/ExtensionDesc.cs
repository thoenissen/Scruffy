namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Describes an extension
    /// </summary>
    public class ExtensionDesc
    {
        /// <summary>
        /// Name of the extension
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Version of the extension, "Unknown" if missing
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Revision of the extension
        /// </summary>
        public ulong Revision { get; set; }

        /// <summary>
        /// Signature of the extension
        /// </summary>
        public ulong Signature { get; set; }

        /// <summary>
        /// List of <see cref="JsonActor.Name"/> running the extension.
        /// </summary>
        public IReadOnlyList<string> RunningExtension { get; set; }
    }
}
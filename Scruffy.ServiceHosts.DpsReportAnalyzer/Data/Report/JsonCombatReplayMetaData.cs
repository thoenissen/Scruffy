namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Combat Replay meta data
    /// </summary>
    public class JsonCombatReplayMetaData
    {
        /// <summary>
        /// Factor to convert inches (in game unit) to pixels (map unit)
        /// </summary>
        public float InchToPixel { get; set; }

        /// <summary>
        /// Polling rate of the time based information (facings, positions, etc...) in ms.
        /// A polling rate of 150 means that 150 ms separates two time based information.
        /// Time based
        /// </summary>
        public int PollingRate { get; set; }

        /// <summary>
        /// Sizes[0] is width of the map in pixel and Sizes[1] is height of the map in pixel.
        /// All maps in <see cref="Maps"/> are of the same pixel size.
        /// </summary>
        public int[] Sizes { get; set; }

        /// <summary>
        /// List of maps used for Combat Replay
        /// </summary>
        /// <seealso cref="CombatReplayMap"/>
        public IReadOnlyList<CombatReplayMap> Maps { get; set; }
    }
}
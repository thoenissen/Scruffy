﻿namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Class representing consumables
    /// </summary>
    public class JsonConsumable
    {
        /// <summary>
        /// Number of stacks
        /// </summary>
        public int Stack { get; set; }

        /// <summary>
        /// Duration of the consumable
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Time of application of the consumable
        /// </summary>
        public long Time { get; set; }

        /// <summary>
        /// ID of the consumable
        /// </summary>
        /// <seealso cref="JsonLog.BuffMap"/>
        public long Id { get; set; }
    }
}
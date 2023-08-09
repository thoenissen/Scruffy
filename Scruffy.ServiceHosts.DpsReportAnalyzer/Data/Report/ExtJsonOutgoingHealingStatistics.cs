﻿namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Outgoing healing statistics
    /// </summary>
    public class ExtJsonOutgoingHealingStatistics
    {
        /// <summary>
        /// Total hps
        /// </summary>
        public int Hps { get; set; }

        /// <summary>
        /// Total healing
        /// </summary>
        public int Healing { get; set; }

        /// <summary>
        /// Total healing power based hps
        /// </summary>
        public int HealingPowerHps { get; set; }

        /// <summary>
        /// Total healing power based healing
        /// </summary>
        public int HealingPowerHealing { get; set; }

        /// <summary>
        /// Total conversion based hps
        /// </summary>
        public int ConversionHps { get; set; }

        /// <summary>
        /// Total conversion based healing
        /// </summary>
        public int ConversionHealing { get; set; }

        /// <summary>
        /// Total hybrid hps
        /// </summary>
        public int HybridHps { get; set; }

        /// <summary>
        /// Total hybrid healing
        /// </summary>
        public int HybridHealing { get; set; }

        /// <summary>
        /// Total hps against downed
        /// </summary>
        public int DownedHps { get; set; }

        /// <summary>
        /// Total healing against downed
        /// </summary>
        public int DownedHealing { get; set; }

        /// <summary>
        /// Total actor only hps
        /// </summary>
        public int ActorHps { get; set; }

        /// <summary>
        /// Total actor only healing
        /// </summary>
        public int ActorHealing { get; set; }

        /// <summary>
        /// Total actor only healing power based hps
        /// </summary>
        public int ActorHealingPowerHps { get; set; }

        /// <summary>
        /// Total actor only healing power based healing
        /// </summary>
        public int ActorHealingPowerHealing { get; set; }

        /// <summary>
        /// Total actor only conversion based hps
        /// </summary>
        public int ActorConversionHps { get; set; }

        /// <summary>
        /// Total actor only conversion based healing
        /// </summary>
        public int ActorConversionHealing { get; set; }

        /// <summary>
        /// Total actor only hybrid hps
        /// </summary>
        public int ActorHybridHps { get; set; }

        /// <summary>
        /// Total actor only hybrid healing
        /// </summary>
        public int ActorHybridHealing { get; set; }

        /// <summary>
        /// Total actor only hps against downed
        /// </summary>
        public int ActorDownedHps { get; set; }

        /// <summary>
        /// Total actor only healing against downed
        /// </summary>
        public int ActorDownedHealing { get; set; }
    }
}
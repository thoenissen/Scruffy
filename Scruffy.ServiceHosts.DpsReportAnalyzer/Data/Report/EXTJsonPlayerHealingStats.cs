﻿namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Class regrouping player related healing statistics
    /// </summary>
    public class ExtJsonPlayerHealingStats
    {
        /// <summary>
        /// Array of Total Allied Outgoing Healing stats
        /// Length == # of players and the length of each sub array is equal to # of phases
        /// </summary>
        /// <seealso cref="ExtJsonOutgoingHealingStatistics"/>
        public IReadOnlyList<IReadOnlyList<ExtJsonOutgoingHealingStatistics>> OutgoingHealingAllies { get; set; }

        /// <summary>
        /// Array of Total Outgoing Healing stats
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="ExtJsonOutgoingHealingStatistics"/>
        public IReadOnlyList<ExtJsonOutgoingHealingStatistics> OutgoingHealing { get; set; }

        /// <summary>
        /// Array of Total Incoming Healing stats
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="ExtJsonIncomingHealingStatistics"/>
        public IReadOnlyList<ExtJsonIncomingHealingStatistics> IncomingHealing { get; set; }

        /// <summary>
        /// Array of int representing 1S outgoing allied healing points
        /// Length == # of players and the length of each sub array is equal to # of phases
        /// </summary>
        /// <remarks>
        /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        /// </remarks>
        public IReadOnlyList<IReadOnlyList<IReadOnlyList<int>>> AlliedHealing1S { get; set; }

        /// <summary>
        /// Array of int representing 1S outgoing allied healing power based healing points
        /// Length == # of players and the length of each sub array is equal to # of phases
        /// </summary>
        /// <remarks>
        /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        /// </remarks>
        public IReadOnlyList<IReadOnlyList<IReadOnlyList<int>>> AlliedHealingPowerHealing1S { get; set; }

        /// <summary>
        /// Array of int representing 1S outgoing allied conversion based healing points
        /// Length == # of players and the length of each sub array is equal to # of phases
        /// </summary>
        /// <remarks>
        /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        /// </remarks>
        public IReadOnlyList<IReadOnlyList<IReadOnlyList<int>>> AlliedConversionHealingHealing1S { get; set; }

        /// <summary>
        /// Array of int representing 1S outgoing allied hybrid healing points
        /// Length == # of players and the length of each sub array is equal to # of phases
        /// </summary>
        /// <remarks>
        /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        /// </remarks>
        public IReadOnlyList<IReadOnlyList<IReadOnlyList<int>>> AlliedHybridHealing1S { get; set; }

        /// <summary>
        /// Array of int representing 1S outgoing healing points
        /// Length == # of phases
        /// </summary>
        /// <remarks>
        /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        /// </remarks>
        public IReadOnlyList<IReadOnlyList<int>> Healing1S { get; set; }

        /// <summary>
        /// Array of int representing 1S outgoing healing power based healing points
        /// Length == # of phases
        /// </summary>
        /// <remarks>
        /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        /// </remarks>
        public IReadOnlyList<IReadOnlyList<int>> HealingPowerHealing1S { get; set; }

        /// <summary>
        /// Array of int representing 1S outgoing conversion based healing points
        /// Length == # of phases
        /// </summary>
        /// <remarks>
        /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        /// </remarks>
        public IReadOnlyList<IReadOnlyList<int>> ConversionHealingHealing1S { get; set; }

        /// <summary>
        /// Array of int representing 1S outgoing hybrid healing points
        /// Length == # of phases
        /// </summary>
        /// <remarks>
        /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point
        /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        /// </remarks>
        public IReadOnlyList<IReadOnlyList<int>> HybridHealing1S { get; set; }

        /// <summary>
        /// Total Outgoing Allied Healing distribution array
        /// Length == # of players and the length of each sub array is equal to # of phases
        /// </summary>
        /// <seealso cref="ExtJsonHealingDist"/>
        public IReadOnlyList<IReadOnlyList<IReadOnlyList<ExtJsonHealingDist>>> AlliedHealingDist { get; set; }

        /// <summary>
        /// Total Outgoing Healing distribution array
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="ExtJsonHealingDist"/>
        public IReadOnlyList<IReadOnlyList<ExtJsonHealingDist>> TotalHealingDist { get; set; }

        /// <summary>
        /// Total Incoming Healing distribution array
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="ExtJsonHealingDist"/>
        public IReadOnlyList<IReadOnlyList<ExtJsonHealingDist>> TotalIncomingHealingDist { get; set; }
    }
}
﻿namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Class corresponding to an actor's combat replay data
    /// </summary>
    public class JsonActorCombatReplayData
    {
        /// <summary>
        /// Time at which the actor becomes active.
        /// </summary>
        public long Start { get; set; }

        /// <summary>
        /// Time at which the actor becomes inactive.
        /// </summary>
        public long End { get; set; }

        /// <summary>
        /// Url to the actor's icon.
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// List of 2D positions in pixels.
        /// The corresponding time for a given index i is ceil(<see cref="Start"/> / <see cref="JsonCombatReplayMetaData.PollingRate"/>) * <see cref="JsonCombatReplayMetaData.PollingRate"/>  + i * <see cref="JsonCombatReplayMetaData.PollingRate"/>
        /// </summary>
        public IReadOnlyList<float[]> Positions { get; set; }

        /// <summary>
        /// List of orientation angles in degree.
        /// The corresponding time for a given index i is ceil(<see cref="Start"/> / <see cref="JsonCombatReplayMetaData.PollingRate"/>) * <see cref="JsonCombatReplayMetaData.PollingRate"/>  + i * <see cref="JsonCombatReplayMetaData.PollingRate"/>.
        /// Can be null.
        /// </summary>
        public IReadOnlyList<float> Orientations { get; set; }

        /// <summary>
        /// List of time intervals between which the actor is dead.
        /// Can be null.
        /// </summary>
        public IReadOnlyList<long[]> Dead { get; set; }

        /// <summary>
        /// List of time intervals between which the actor is in downstate.
        /// Can be null.
        /// ///
        /// </summary>
        public IReadOnlyList<long[]> Down { get; set; }

        /// <summary>
        /// List of time intervals between which the actor is disconnected/not spawned.
        /// Can be null.
        /// </summary>
        public IReadOnlyList<long[]> Dc { get; set; }
    }
}
using System.Collections.Generic;

namespace Scruffy.Data.Services.Raid
{
    /// <summary>
    /// Appointment slot data
    /// </summary>
    public class RaidAppointmentSlotData
    {
        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Discord emoji
        /// </summary>
        public ulong DiscordEmoji { get; set; }

        /// <summary>
        /// Valid experience levels
        /// </summary>
        public List<long> ExperienceLevelIds { get; set; }

        /// <summary>
        /// Users
        /// </summary>
        public List<ulong> Users { get; set; }

        /// <summary>
        /// Slot count
        /// </summary>
        public long SlotCount { get; set; }
    }
}
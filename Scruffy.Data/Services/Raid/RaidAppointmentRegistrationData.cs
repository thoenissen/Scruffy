namespace Scruffy.Data.Services.Raid
{
    /// <summary>
    /// Appointment registration data
    /// </summary>
    public class RaidAppointmentRegistrationData
    {
        /// <summary>
        /// Id of the user
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        /// Experience level id
        /// </summary>
        public long? RaidExperienceLevelId { get; set; }
    }
}
using System;

namespace Scruffy.Data.Services.Fractal
{
    /// <summary>
    /// Data of the registration
    /// </summary>
    public class AppointmentCreationRegistrationData
    {
        /// <summary>
        /// Timestamp of the appointment
        /// </summary>
        public DateTime AppointmentTimeStamp { get; set; }

        /// <summary>
        /// Id of the user
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        /// Timestamp of the registration
        /// </summary>
        public DateTime RegistrationTimeStamp { get; set; }
    }
}

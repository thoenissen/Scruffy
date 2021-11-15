using System;

namespace Scruffy.Data.Services.Fractal;

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
    public long UserId { get; set; }

    /// <summary>
    /// Id of the discord account
    /// </summary>
    public ulong DiscordAccountId { get; set; }

    /// <summary>
    /// Timestamp of the registration
    /// </summary>
    public DateTime RegistrationTimeStamp { get; set; }
}
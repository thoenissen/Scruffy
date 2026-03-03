using System;

namespace Scruffy.WebApp.DTOs.Raid;

/// <summary>
/// Appointment data for the raid commit page selector
/// </summary>
public record RaidAppointmentDTO
{
    /// <summary>
    /// Appointment ID
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Configuration ID
    /// </summary>
    public long ConfigurationId { get; init; }

    /// <summary>
    /// Timestamp of the appointment
    /// </summary>
    public DateTime TimeStamp { get; init; }

    /// <summary>
    /// Registration deadline
    /// </summary>
    public DateTime Deadline { get; init; }

    /// <summary>
    /// Whether the appointment has already been committed
    /// </summary>
    public bool IsCommitted { get; init; }
}
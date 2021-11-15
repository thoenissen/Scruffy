namespace Scruffy.Data.Services.Raid;

/// <summary>
/// Appointment slot data
/// </summary>
public class RaidAppointmentSlotData
{
    /// <summary>
    /// Rank
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// Users
    /// </summary>
    public List<long> Registrations { get; set; }

    /// <summary>
    /// Slot count
    /// </summary>
    public long SlotCount { get; set; }

    /// <summary>
    /// Experience level id
    /// </summary>
    public long ExperienceLevelId { get; set; }
}
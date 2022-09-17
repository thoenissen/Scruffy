namespace Scruffy.ServiceHosts.WebApi.DTO.Raid;

/// <summary>
/// Line up
/// </summary>
public class LineUpDTO
{
    #region Properties

    /// <summary>
    /// Id of the appointment
    /// </summary>
    public long AppointmentId { get; set; }

    /// <summary>
    /// Groups
    /// </summary>
    public Dictionary<int, List<LineUpEntryDTO>> Groups { get; set; }

    #endregion // Properties
}
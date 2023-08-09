namespace Scruffy.ServiceHosts.WebApi.DTO.Raid;

/// <summary>
/// Active raid appointment
/// </summary>
public class ActiveRaidAppointmentDTO
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Time stamp
    /// </summary>
    public DateTime TimeStamp { get; set; }

    /// <summary>
    /// Title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Participants
    /// </summary>
    public List<RaidParticipantDTO> Participants { get; set; }
}
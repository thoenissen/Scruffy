namespace Scruffy.Data.Services.Raid;

/// <summary>
/// Raid appointment message data
/// </summary>
public class RaidAppointmentMessageData
{
    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime TimeStamp { get; set; }

    /// <summary>
    /// Id of the channel
    /// </summary>
    public ulong ChannelId { get; set; }

    /// <summary>
    /// Id of the message
    /// </summary>
    public ulong MessageId { get; set; }

    /// <summary>
    /// Thumbnail
    /// </summary>
    public string Thumbnail { get; set; }

    /// <summary>
    /// Title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Experience levels
    /// </summary>
    public List<RaidAppointmentMessageExperienceLevel> ExperienceLevels { get; set; }

    /// <summary>
    /// Registrations
    /// </summary>
    public List<RaidAppointmentRegistrationData> Registrations { get; set; }
}
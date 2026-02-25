namespace Scruffy.ServiceHosts.Discord.Endpoints;

/// <summary>
/// Request to schedule raid message refresh jobs
/// </summary>
public record RaidMessageRefreshRequest
{
    #region Constructor

    /// <summary>
    /// Request to schedule raid message refresh jobs
    /// </summary>
    /// <param name="configurationId">ID of the raid configuration</param>
    /// <param name="deadline">Deadline timestamp for the first job</param>
    /// <param name="timeStamp">Appointment timestamp for the second job</param>
    public RaidMessageRefreshRequest(long configurationId, DateTime deadline, DateTime timeStamp)
    {
        ConfigurationId = configurationId;
        Deadline = deadline;
        TimeStamp = timeStamp;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// ID of the raid configuration
    /// </summary>
    public long ConfigurationId { get; }

    /// <summary>
    /// Deadline timestamp for the first job
    /// </summary>
    public DateTime Deadline { get; }

    /// <summary>
    /// Appointment timestamp for the second job
    /// </summary>
    public DateTime TimeStamp { get; }

    #endregion // Properties
}
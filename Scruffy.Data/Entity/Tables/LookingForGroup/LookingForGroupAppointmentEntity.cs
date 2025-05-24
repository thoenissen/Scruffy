using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.LookingForGroup;

/// <summary>
/// Looking for group appointment
/// </summary>
[Table("LookingForGroupAppointments")]
public class LookingForGroupAppointmentEntity
{
    #region Properties

    /// <summary>
    /// Id
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Date of the appointment
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// Number of participants
    /// </summary>
    public int? ParticipantCount { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Id the creator
    /// </summary>
    public long CreationUserId { get; set; }

    /// <summary>
    /// Id of the channel
    /// </summary>
    public ulong ChannelId { get; set; }

    /// <summary>
    /// id of the message
    /// </summary>
    public ulong MessageId { get; set; }

    /// <summary>
    /// Id of the thread
    /// </summary>
    public ulong? ThreadId { get; set; }

    #region Navigation properties

    /// <summary>
    /// Creation user
    /// </summary>
    [ForeignKey(nameof(CreationUserId))]
    public virtual UserEntity CreationUser { get; set; }

    /// <summary>
    /// Participants
    /// </summary>
    public virtual ICollection<LookingForGroupParticipantEntity> Participants { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}
using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.LookingForGroup;

/// <summary>
/// Participants of a looking for group appointment
/// </summary>
[Table("LookingForGroupParticipants")]
public class LookingForGroupParticipantEntity
{
    #region Properties

    /// <summary>
    /// Id of the appointment
    /// </summary>
    public int AppointmentId { get; set; }

    /// <summary>
    /// Id of the user
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Registration timestamp
    /// </summary>
    public DateTime RegistrationTimeStamp { get; set; }

    #region Navigation properties

    /// <summary>
    /// Appointment
    /// </summary>
    [ForeignKey(nameof(AppointmentId))]
    public virtual LookingForGroupAppointmentEntity Appointment { get; set; }

    /// <summary>
    /// User
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual UserEntity User { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}
using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;
using Scruffy.Data.Enumerations.Raid;

namespace Scruffy.Data.Entity.Tables.Raid;

/// <summary>
/// Line up squad
/// </summary>
[Table("RaidAppointmentLineUpSquads")]
public class RaidAppointmentLineUpSquadEntity
{
    #region Properties

    #region Key

    /// <summary>
    /// Appointment ID
    /// </summary>
    public long AppointmentId { get; set; }

    /// <summary>
    /// Group Number
    /// </summary>
    public int GroupNumber { get; set; }

    /// <summary>
    /// Message ID
    /// </summary>
    public ulong MessageId { get; set; }

    #endregion // Key

    #region Group 1

    /// <summary>
    /// Tank User ID
    /// </summary>
    public long? TankUserId { get; set; }

    /// <summary>
    /// Tank Raid Role
    /// </summary>
    public RaidRole TankRaidRole { get; set; }

    /// <summary>
    /// Support 1 User ID
    /// </summary>
    public long? Support1UserId { get; set; }

    /// <summary>
    /// DPS 1 User ID
    /// </summary>
    public long? Dps1UserId { get; set; }

    /// <summary>
    /// DPS 2 User ID
    /// </summary>
    public long? Dps2UserId { get; set; }

    /// <summary>
    /// DPS 3 User ID
    /// </summary>
    public long? Dps3UserId { get; set; }

    #endregion // Group 1

    #region Group 2

    /// <summary>
    /// Healer User ID
    /// </summary>
    public long? HealerUserId { get; set; }

    /// <summary>
    /// Healer Raid Role
    /// </summary>
    public RaidRole HealerRaidRole { get; set; }

    /// <summary>
    /// Support 2 User ID
    /// </summary>
    public long? Support2UserId { get; set; }

    /// <summary>
    /// DPS 1 User ID
    /// </summary>
    public long? Dps4UserId { get; set; }

    /// <summary>
    /// DPS 2 User ID
    /// </summary>
    public long? Dps5UserId { get; set; }

    /// <summary>
    /// DPS 3 User ID
    /// </summary>
    public long? Dps6UserId { get; set; }

    #endregion // Group 2

    #region Navigation properties

    #region Key

    /// <summary>
    /// Appointment
    /// </summary>
    [ForeignKey(nameof(AppointmentId))]
    public RaidAppointmentEntity RaidAppointment { get; set; }

    #endregion // Key

    #region Group 1

    /// <summary>
    /// Tank
    /// </summary>
    [ForeignKey(nameof(TankUserId))]
    public UserEntity Tank { get; set; }

    /// <summary>
    /// Support 1
    /// </summary>
    [ForeignKey(nameof(Support1UserId))]
    public UserEntity Support1 { get; set; }

    /// <summary>
    /// DPS 1
    /// </summary>
    [ForeignKey(nameof(Dps1UserId))]
    public UserEntity Dps1 { get; set; }

    /// <summary>
    /// DPS 2
    /// </summary>
    [ForeignKey(nameof(Dps2UserId))]
    public UserEntity Dps2 { get; set; }

    /// <summary>
    /// DPS 3
    /// </summary>
    [ForeignKey(nameof(Dps3UserId))]
    public UserEntity Dps3 { get; set; }

    #endregion // Group 1

    #region Group 2

    /// <summary>
    /// Healer
    /// </summary>
    [ForeignKey(nameof(HealerUserId))]
    public UserEntity Healer { get; set; }

    /// <summary>
    /// Support 2
    /// </summary>
    [ForeignKey(nameof(Support2UserId))]
    public UserEntity Support2 { get; set; }

    /// <summary>
    /// DPS 4
    /// </summary>
    [ForeignKey(nameof(Dps4UserId))]
    public UserEntity Dps4 { get; set; }

    /// <summary>
    /// DPS 5
    /// </summary>
    [ForeignKey(nameof(Dps5UserId))]
    public UserEntity Dps5 { get; set; }

    /// <summary>
    /// DPS 6
    /// </summary>
    [ForeignKey(nameof(Dps6UserId))]
    public UserEntity Dps6 { get; set; }

    #endregion // Group 2

    #endregion // Navigation properties

    #endregion // Properties
}
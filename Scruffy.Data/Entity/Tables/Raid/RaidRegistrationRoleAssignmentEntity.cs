using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Raid;

/// <summary>
/// Assignment of roles to an registration
/// </summary>
[Table("RaidRegistrationRoleAssignments")]
public class RaidRegistrationRoleAssignmentEntity
{
    #region Properties

    /// <summary>
    /// Id
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Id of the registration
    /// </summary>
    public long RegistrationId { get; set; }

    /// <summary>
    /// Id of the main role
    /// </summary>
    public long? MainRoleId { get; set; }

    /// <summary>
    /// Id of the sub role
    /// </summary>
    public long? SubRoleId { get; set; }

    #region Navigation properties

    /// <summary>
    /// Registration
    /// </summary>
    [ForeignKey(nameof(RegistrationId))]
    public virtual RaidRegistrationEntity RaidRegistration { get; set; }

    /// <summary>
    /// Main role
    /// </summary>
    [ForeignKey(nameof(MainRoleId))]
    public virtual RaidRoleEntity MainRaidRole { get; set; }

    /// <summary>
    /// Sub role
    /// </summary>
    [ForeignKey(nameof(SubRoleId))]
    public virtual RaidRoleEntity SubRaidRole { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}
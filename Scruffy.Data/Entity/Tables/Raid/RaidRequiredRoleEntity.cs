using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Raid;

/// <summary>
/// Assigning the required roles
/// </summary>
[Table("RaidRequiredRoles")]
public class RaidRequiredRoleEntity
{
    #region Properties

    /// <summary>
    /// Id of the template
    /// </summary>
    public long TemplateId { get; set; }

    /// <summary>
    /// Index
    /// </summary>
    public long Index { get; set; }

    /// <summary>
    /// Id of the main role
    /// </summary>
    public long MainRoleId { get; set; }

    /// <summary>
    /// Id of the sub role
    /// </summary>
    public long? SubRoleId { get; set; }

    #region Navigation properties

    /// <summary>
    /// Configuration
    /// </summary>
    [ForeignKey(nameof(TemplateId))]
    public virtual RaidDayTemplateEntity RaidDayTemplateEntity { get; set; }

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
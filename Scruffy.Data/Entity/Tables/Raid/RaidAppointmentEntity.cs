using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Raid;

/// <summary>
/// Appointment
/// </summary>
[Table("RaidAppointments")]
public class RaidAppointmentEntity
{
    #region Properties

    /// <summary>
    /// Id
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Id of the configuration
    /// </summary>
    public long ConfigurationId { get; set; }

    /// <summary>
    /// Id of the template
    /// </summary>
    public long TemplateId { get; set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime TimeStamp { get; set; }

    /// <summary>
    /// Registration deadline
    /// </summary>
    public DateTime Deadline { get; set; }

    /// <summary>
    /// Is the appointment already commit?
    /// </summary>
    public bool IsCommitted { get; set; }

    #region Navigation properties

    /// <summary>
    /// Configuration
    /// </summary>
    [ForeignKey(nameof(ConfigurationId))]
    public virtual RaidDayConfigurationEntity RaidDayConfiguration { get; set; }

    /// <summary>
    /// Template
    /// </summary>
    [ForeignKey(nameof(TemplateId))]
    public virtual RaidDayTemplateEntity RaidDayTemplate { get; set; }

    /// <summary>
    /// Registrations
    /// </summary>
    public virtual ICollection<RaidRegistrationEntity> RaidRegistrations { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}
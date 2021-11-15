using System;
using System.ComponentModel.DataAnnotations.Schema;
using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.Fractals;

/// <summary>
/// Registration of an fractal appointment
/// </summary>
[Table("FractalRegistrations")]
public class FractalRegistrationEntity
{
    #region Properties

    /// <summary>
    /// Id of the configuration
    /// </summary>
    public long ConfigurationId { get; set; }

    /// <summary>
    /// Timestamp of the appointment
    /// </summary>
    public DateTime AppointmentTimeStamp { get; set; }

    /// <summary>
    /// Id of the user
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Timestamp of the registration
    /// </summary>
    public DateTime RegistrationTimeStamp { get; set; }

    /// <summary>
    /// Id of the created appointment
    /// </summary>
    public long? AppointmentId { get; set; }

    #region Navigation properties

    /// <summary>
    /// Configuration
    /// </summary>
    [ForeignKey(nameof(ConfigurationId))]
    public virtual FractalLfgConfigurationEntity FractalLfgConfiguration { get; set; }

    /// <summary>
    /// User
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual UserEntity User { get; set; }

    /// <summary>
    /// Appointment
    /// </summary>
    [ForeignKey(nameof(AppointmentId))]
    public virtual FractalAppointmentEntity FractalAppointmentEntity { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}
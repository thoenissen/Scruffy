using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Fractals;

/// <summary>
/// Data of the appointment
/// </summary>
[Table("FractalAppointments")]
public class FractalAppointmentEntity
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
    /// Id of the message
    /// </summary>
    public ulong DiscordMessageId { get; set; }

    /// <summary>
    /// Appointment timestamp
    /// </summary>
    public DateTime AppointmentTimeStamp { get; set; }

    #region Navigation properties

    /// <summary>
    /// Configuration
    /// </summary>
    [ForeignKey(nameof(ConfigurationId))]
    public virtual FractalLfgConfigurationEntity FractalLfgConfiguration { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}
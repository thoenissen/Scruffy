using System.ComponentModel.DataAnnotations.Schema;
using Scruffy.Data.Entity.Tables.CoreData;
using Scruffy.Data.Enumerations.Guild;

namespace Scruffy.Data.Entity.Tables.Guild;

/// <summary>
/// Special rank protocol
/// </summary>
[Table("GuildSpecialRankProtocolEntries")]
public class GuildSpecialRankProtocolEntryEntity
{
    #region Properties

    /// <summary>
    /// Id
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime TimeStamp { get; set; }

    /// <summary>
    /// Id of the configuration
    /// </summary>
    public long ConfigurationId { get; set; }

    /// <summary>
    /// Typ
    /// </summary>
    public GuildSpecialRankProtocolEntryType Type { get; set; }

    /// <summary>
    /// Id of the user
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// Amount of points
    /// </summary>
    public double? Amount { get; set; }

    #region Navigation properties

    /// <summary>
    /// Configuration
    /// </summary>
    [ForeignKey(nameof(ConfigurationId))]
    public virtual GuildSpecialRankConfigurationEntity GuildSpecialRankConfiguration { get; set; }

    /// <summary>
    /// User
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual UserEntity User { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}
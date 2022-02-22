using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Scruffy.Data.Enumerations.GuildWars2;

namespace Scruffy.Data.Entity.Tables.GuildWars2.GameData;

/// <summary>
/// Item
/// </summary>
[Table("GuildWarsItems")]
public class GuildWarsItemEntity
{
    /// <summary>
    /// Id of the item
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int ItemId { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Type
    /// </summary>
    public GuildWars2ItemType Type { get; set; }

    /// <summary>
    /// Vendor value
    /// </summary>
    public long? VendorValue { get; set; }

    /// <summary>
    /// Custom value
    /// </summary>
    public long? CustomValue { get; set; }

    /// <summary>
    /// Valid date of the custom value
    /// </summary>
    public DateTime? CustomValueValidDate { get; set; }

    /// <summary>
    /// Should the value be reduced after n inserts.
    /// </summary>
    public bool IsCustomValueThresholdActivated { get; set; }
}
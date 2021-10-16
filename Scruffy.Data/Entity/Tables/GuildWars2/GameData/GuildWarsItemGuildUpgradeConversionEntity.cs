using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.GuildWars2.GameData
{
    /// <summary>
    /// Item to guild upgrade conversion
    /// </summary>
    [Table("GuildWarsItemGuildUpgradeConversions")]
    public class GuildWarsItemGuildUpgradeConversionEntity
    {
        #region Properties

        /// <summary>
        /// Id of the item
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Id of the upgrade
        /// </summary>
        public long UpgradeId { get; set; }

        #region Navigation properties

        /// <summary>
        /// Item
        /// </summary>
        [ForeignKey(nameof(ItemId))]
        public virtual GuildWarsItemEntity Item { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}
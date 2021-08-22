using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.GuildWars2
{
    /// <summary>
    /// World
    /// </summary>
    [Table("GuildWarsWorlds")]
    public class GuildWarsWorldEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        #endregion // Properties
    }
}

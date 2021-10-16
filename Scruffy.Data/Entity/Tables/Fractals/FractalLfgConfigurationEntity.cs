using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Fractals
{
    /// <summary>
    /// Configuration of a fractal lfg
    /// </summary>
    [Table("FractalLfgConfigurations")]
    public class FractalLfgConfigurationEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Id of the channel
        /// </summary>
        public ulong DiscordChannelId { get; set; }

        /// <summary>
        /// Id of the discord message
        /// </summary>
        public ulong DiscordMessageId { get; set; }

        /// <summary>
        /// Alias name
        /// </summary>
        [StringLength(20)]
        public string AliasName { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        #region Navigation properties

        /// <summary>
        /// Registrations
        /// </summary>
        public virtual ICollection<FractalRegistrationEntity> FractalRegistrations { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Raid
{
    /// <summary>
    /// Raid day template
    /// </summary>
    [Table("RaidDayTemplates")]
    public class RaidDayTemplateEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Alias name
        /// </summary>
        public string AliasName { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Thumbnail
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        /// Ist the template deleted?
        /// </summary>
        public bool IsDeleted { get; set; }

        #endregion // Properties
    }
}

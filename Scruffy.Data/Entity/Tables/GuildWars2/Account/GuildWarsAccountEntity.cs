using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.GuildWars2.Account
{
    /// <summary>
    /// Account
    /// </summary>
    [Table("GuildWarsAccounts")]
    public class GuildWarsAccountEntity
    {
        #region Properties

        /// <summary>
        /// Account name
        /// </summary>
        [Key]
        [StringLength(42)]
        public string Name { get; set; }

        /// <summary>
        /// Id of the user
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// dps.report user token
        /// </summary>
        public string DpsReportUserToken { get; set; }

        /// <summary>
        /// Api key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Last age in seconds
        /// </summary>
        public long LastAge { get; set; }

        /// <summary>
        /// Id of the word
        /// </summary>
        public long? WorldId { get; set; }

        #region Navigation properties

        /// <summary>
        /// User
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public UserEntity User { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}

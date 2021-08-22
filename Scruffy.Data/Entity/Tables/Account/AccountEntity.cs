using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.Account
{
    /// <summary>
    /// Account
    /// </summary>
    [Table("Accounts")]
    public class AccountEntity
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
        public ulong UserId { get; set; }

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
        public long? WordId { get; set; }

        #region Navigation properties

        /// <summary>
        /// User
        /// </summary>
        public UserEntity User { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}

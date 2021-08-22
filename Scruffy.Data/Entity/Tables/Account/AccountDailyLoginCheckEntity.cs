using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Account
{
    /// <summary>
    /// Daily logins
    /// </summary>
    [Table("AccountDailyLoginChecks")]
    public class AccountDailyLoginCheckEntity
    {
        #region Properties

        /// <summary>
        /// Account name
        /// </summary>
        [StringLength(42)]
        public string Name { get; set; }

        /// <summary>
        /// Day
        /// </summary>
        public DateTime Date { get; set; }

        #region Navigation properties

        /// <summary>
        /// Account
        /// </summary>
        [ForeignKey(nameof(Name))]
        public AccountEntity AccountEntity { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}
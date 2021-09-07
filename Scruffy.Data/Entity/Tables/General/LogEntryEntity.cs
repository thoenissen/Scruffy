using System;
using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Enumerations.General;

namespace Scruffy.Data.Entity.Tables.General
{
    /// <summary>
    /// Log entries
    /// </summary>
    [Table("LogEntries")]
    public class LogEntryEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Time stamp
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public LogEntryType Type { get; set; }

        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Last user command
        /// </summary>
        public string LastUserCommand { get; set; }

        /// <summary>
        /// Command name
        /// </summary>
        public string QualifiedCommandName { get; set; }

        #endregion // Properties
    }
}

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
        /// Level
        /// </summary>
        public LogEntryLevel Level { get; set; }

        /// <summary>
        /// Source
        /// <Remarks>
        /// Command: Command name
        /// Job:     Class name
        /// </Remarks>
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Additional source specification
        /// Last user command
        /// </summary>
        public string SubSource { get; set; }

        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Additional information
        /// </summary>
        public string AdditionalInformation { get; set; }

        #endregion // Properties
    }
}

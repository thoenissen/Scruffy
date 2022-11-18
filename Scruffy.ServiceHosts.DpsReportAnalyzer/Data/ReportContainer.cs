using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report;
using Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Upload;

namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data
{
    /// <summary>
    /// Report container
    /// </summary>
    public class ReportContainer
    {
        /// <summary>
        /// Id
        /// </summary>
        [BsonId]
        public ObjectId Id { get; set; }

        /// <summary>
        /// User token
        /// </summary>
        public string UserToken { get; set; }

        /// <summary>
        /// MetaData
        /// </summary>
        public MetaData MetaData { get; set; }

        /// <summary>
        /// Log
        /// </summary>
        public JsonLog Details { get; set; }
    }
}
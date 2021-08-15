using System.Collections.Generic;

using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart
{
    /// <summary>
    /// Collection of annotations
    /// </summary>
    public class AnnotationsCollection
    {
        /// <summary>
        /// Annotations
        /// </summary>
        [JsonProperty("annotations")]
        public List<Annotation> Annotations { get; set; }
    }
}
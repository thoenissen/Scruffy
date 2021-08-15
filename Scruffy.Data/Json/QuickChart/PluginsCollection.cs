using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart
{
    /// <summary>
    /// Plugins
    /// </summary>
    public class PluginsCollection
    {
        /// <summary>
        /// Legend
        /// </summary>
        [JsonProperty("legend")]
        public bool Legend { get; set; }
    }
}
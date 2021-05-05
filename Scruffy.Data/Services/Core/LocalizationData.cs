using System.Collections.Generic;

namespace Scruffy.Data.Services.Core
{
    /// <summary>
    /// Localization data
    /// </summary>
    public class LocalizationData
    {
        /// <summary>
        /// Culture
        /// </summary>
        public string CultureInfo { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Groups
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> TranslationGroups { get; set; }
    }
}
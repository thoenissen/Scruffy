using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using Scruffy.Data.Services.Core;

namespace Scruffy.Services.Core
{
    /// <summary>
    /// Providing located string
    /// </summary>
    public class LocalizationService
    {
        #region Fields

        /// <summary>
        /// Groups
        /// </summary>
        private ConcurrentDictionary<string, LocalizationGroup> _groups;

        #endregion // Fields

        #region Properties

        /// <summary>
        /// Culture
        /// </summary>
        public CultureInfo Culture { get; private set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Loading the located data
        /// </summary>
        /// <param name="stream">Stream</param>
        public void Load(Stream stream)
        {
            var data = JsonConvert.DeserializeObject<LocalizationData>(new StreamReader(stream).ReadToEnd());

            Culture = CultureInfo.CreateSpecificCulture(data.CultureInfo);

            _groups = new ConcurrentDictionary<string, LocalizationGroup>(data.TranslationGroups.ToDictionary(obj => obj.Key, obj => new LocalizationGroup(this, obj.Value)));
        }

        /// <summary>
        /// Return a group by the given key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>The <see cref="LocalizationGroup"/> matching the given key.</returns>
        public LocalizationGroup GetGroup(string key)
        {
            if (_groups.TryGetValue(key, out var group) == false)
            {
                group = new LocalizationGroup(this);
            }

            return group;
        }

        #endregion // Methods
    }
}

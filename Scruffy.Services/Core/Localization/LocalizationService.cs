using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Reflection;

using Newtonsoft.Json;

using Scruffy.Data.Services.Core;

namespace Scruffy.Services.Core.Localization;

/// <summary>
/// Providing located string
/// </summary>
public class LocalizationService : SingletonLocatedServiceBase
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

    #region SingletonLocatedServiceBase

    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <remarks>When this method is called all services are registered and can be resolved.  But not all singleton services may be initialized. </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task Initialize(IServiceProvider serviceProvider)
    {
        await base.Initialize(serviceProvider)
                  .ConfigureAwait(false);

        // TODO configuration
        var stream = Assembly.Load("Scruffy.Data").GetManifestResourceStream("Scruffy.Data.Resources.Languages.de-DE.json");
        if (stream != null)
        {
            await using (stream.ConfigureAwait(false))
            {
                Load(stream);
            }
        }
    }

    #endregion // SingletonLocatedServiceBase
}
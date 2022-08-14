using System.Collections.Concurrent;
using System.Globalization;

namespace Scruffy.Services.Core.Localization;

/// <summary>
/// Located group of texts
/// </summary>
public class LocalizationGroup
{
    #region Fields

    /// <summary>
    /// Text
    /// </summary>
    private ConcurrentDictionary<string, string> _texts;

    /// <summary>
    /// Service
    /// </summary>
    private LocalizationService _service;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="service">Service</param>
    public LocalizationGroup(LocalizationService service)
        : this(service, null)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="service">Service</param>
    /// <param name="texts">Texts</param>
    public LocalizationGroup(LocalizationService service, IReadOnlyDictionary<string, string> texts)
    {
        _service = service;
        _texts = texts != null ? new ConcurrentDictionary<string, string>(texts) : new ConcurrentDictionary<string, string>();
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Culture
    /// </summary>
    public CultureInfo CultureInfo => _service.Culture;

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Get a text by the given key
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="fallback">Fallback value</param>
    /// <returns>The determined text</returns>
    public string GetText(string key, string fallback)
    {
        if (_texts.TryGetValue(key, out var text) == false)
        {
            text = fallback;
        }

        return text;
    }

    /// <summary>
    /// Get a text by the given key
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <returns>Could a value be determined?</returns>
    public bool TryGetText(string key, out string value) => _texts.TryGetValue(key, out value);

    /// <summary>
    /// Get a text by the given key
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="fallback">Fallback value</param>
    /// <param name="args">Arguments to call <see cref="string.Format(string, object?[])"/></param>
    /// <returns>The determined text</returns>
    public string GetFormattedText(string key, string fallback, params object[] args)
    {
        return string.Format(CultureInfo, GetText(key, fallback), args);
    }

    #endregion // Methods
}
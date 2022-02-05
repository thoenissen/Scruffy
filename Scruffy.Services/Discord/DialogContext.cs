using Discord;

namespace Scruffy.Services.Discord;

/// <summary>
/// Dialog context
/// </summary>
public class DialogContext
{
    #region Fields

    /// <summary>
    /// Values
    /// </summary>
    private Dictionary<string, object> _values;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public DialogContext()
    {
        _values = new Dictionary<string, object>();
        Messages = new List<IMessage>();
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Message
    /// </summary>
    public List<IMessage> Messages { get; }

    /// <summary>
    /// Set value
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    public void SetValue<T>(string key, T value)
    {
        _values[key] = value;
    }

    /// <summary>
    /// Get value
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="key">Key</param>
    /// <returns>Value</returns>
    public T GetValue<T>(string key)
    {
        return (T)_values[key];
    }

    #endregion // Properties
}
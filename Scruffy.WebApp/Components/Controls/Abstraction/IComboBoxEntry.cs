namespace Scruffy.WebApp.Components.Controls.Abstraction;

/// <summary>
/// Interface for a combo box entry
/// </summary>
public interface IComboBoxEntry
{
    /// <summary>
    /// Group name of the entry
    /// </summary>
    public string Group { get; }

    /// <summary>
    /// Indicates if the entry is disabled
    /// </summary>
    public bool IsDisabled { get; }

    /// <summary>
    /// Checks if the current entry is equal to another entry
    /// </summary>
    /// <param name="other">Other entry</param>
    /// <returns>Is the entry equal to <paramref name="other"/>?</returns>
    public bool IsEqualsTo(IComboBoxEntry other);
}
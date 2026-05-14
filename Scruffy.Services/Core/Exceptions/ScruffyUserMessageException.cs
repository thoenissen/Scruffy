namespace Scruffy.Services.Core.Exceptions;

/// <summary>
/// Exception which should be displayed to the user
/// </summary>
public abstract class ScruffyUserMessageException : ScruffyException
{
    #region Methods

    /// <summary>
    /// Returns localized message
    /// </summary>
    /// <returns>Message</returns>
    public abstract string GetLocalizedMessage();

    #endregion // Methods
}
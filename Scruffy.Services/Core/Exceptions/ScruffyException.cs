using System;

namespace Scruffy.Services.Core.Exceptions;

/// <summary>
/// Exception base class
/// </summary>
public abstract class ScruffyException : Exception
{
    /// <summary>
    /// Returns localized message
    /// </summary>
    /// <returns>Message</returns>
    public abstract string GetLocalizedMessage();
}
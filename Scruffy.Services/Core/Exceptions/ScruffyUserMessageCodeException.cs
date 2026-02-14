using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Core.Exceptions;

/// <summary>
/// Exception with a standard user message
/// </summary>
public class ScruffyUserMessageCodeException : ScruffyUserMessageException
{
    #region Fields

    /// <summary>
    /// Code
    /// </summary>
    private readonly ScruffyUserMessageCodeExceptionCode _code;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code">Code</param>
    private ScruffyUserMessageCodeException(ScruffyUserMessageCodeExceptionCode code)
    {
        _code = code;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Throw an exception with the given code
    /// </summary>
    /// <param name="code">Code</param>
    [DoesNotReturn]
    internal static void Throw(ScruffyUserMessageCodeExceptionCode code)
    {
        throw new ScruffyUserMessageCodeException(code);
    }

    #endregion // Methods

    #region ScruffyException

    /// <inheritdoc/>
    public override string GetLocalizedMessage()
    {
        using (var serviceProvider = ServiceProviderContainer.Current.GetServiceProvider())
        {
            return serviceProvider.GetService<LocalizationService>()
                                  ?.GetGroup(nameof(ScruffyUserMessageCodeException))
                                  .GetText(_code.ToString(), "An unknown error is occured");
        }
    }

    #endregion // ScruffyException
}
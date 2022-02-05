using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Core.Exceptions;

/// <summary>
/// Timeout
/// </summary>
public class ScruffyTimeoutException : ScruffyException
{
    #region ScruffyException

    /// <summary>
    /// Returns localized message
    /// </summary>
    /// <returns>Message</returns>
    public override string GetLocalizedMessage()
    {
        using (var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider())
        {
            return serviceProvider.GetService<LocalizationService>()
                                  ?.GetGroup(nameof(ScruffyTimeoutException))
                                  .GetFormattedText("CommandTimeout", "The processing of the command was aborted due to a timeout.");
        }
    }

    #endregion // ScruffyException
}
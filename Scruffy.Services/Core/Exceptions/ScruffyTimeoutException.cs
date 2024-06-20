using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Core.Exceptions;

/// <summary>
/// Timeout
/// </summary>
public class ScruffyTimeoutException : ScruffyUserMessageException
{
    #region ScruffyException

    /// <inheritdoc/>
    public override string GetLocalizedMessage()
    {
        using (var serviceProvider = ServiceProviderContainer.Current.GetServiceProvider())
        {
            return serviceProvider.GetService<LocalizationService>()
                                  ?.GetGroup(nameof(ScruffyTimeoutException))
                                  .GetFormattedText("CommandTimeout", "The processing of the command was aborted due to a timeout.");
        }
    }

    #endregion // ScruffyException
}
using Microsoft.Extensions.DependencyInjection;

namespace Scruffy.Services.Core.JobScheduler;

/// <summary>
/// <see cref="IServiceScope"/> support
/// </summary>
public interface IServiceScopeSupport
{
    #region Methods

    /// <summary>
    /// Set the current scope
    /// </summary>
    /// <param name="scope">scope</param>
    public void SetScope(IServiceScope scope);

    #endregion // Methods
}
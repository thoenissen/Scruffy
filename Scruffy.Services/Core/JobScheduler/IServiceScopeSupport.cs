using Microsoft.Extensions.DependencyInjection;

namespace Scruffy.Services.Core.JobScheduler
{
    /// <summary>
    /// <see cref="IServiceScope"/> support
    /// </summary>
    internal interface IServiceScopeSupport
    {
        /// <summary>
        /// Set the current scope
        /// </summary>
        /// <param name="scope">scope</param>
        void SetScope(IServiceScope scope);
    }
}

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Raid.Jobs;

/// <summary>
/// Refreshing of the raid lfg messages
/// </summary>
public class RaidMessageRefreshJob : LocatedAsyncJob
{
    #region Fields

    /// <summary>
    /// Id of the raid configuration
    /// </summary>
    private readonly long _configurationId;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configurationId">Id of the raid configuration</param>
    public RaidMessageRefreshJob(long configurationId)
    {
        _configurationId = configurationId;
    }

    #endregion // Constructor
    #region LocatedAsyncJob

    /// <inheritdoc/>
    public override async Task ExecuteOverrideAsync()
    {
        if (_configurationId > 0)
        {
            var serviceProvider = ServiceProviderContainer.Current.GetServiceProvider();

            await using (serviceProvider.ConfigureAwait(false))
            {
                await serviceProvider.GetRequiredService<RaidMessageBuilder>()
                                     .RefreshMessageAsync(_configurationId)
                                     .ConfigureAwait(false);
            }
        }
    }

    #endregion // LocatedAsyncJob
}
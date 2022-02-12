using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Fractals;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Fractals.Jobs;

/// <summary>
/// Daily creation of the fractal appointments
/// </summary>
public class FractalDailyRefreshJob : LocatedAsyncJob
{
    #region AsyncJob

    /// <summary>
    /// Executes the job
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task ExecuteOverrideAsync()
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var configurations = await dbFactory.GetRepository<FractalLfgConfigurationRepository>()
                                                .GetQuery()
                                                .Select(obj => new
                                                {
                                                    obj.Id
                                                })
                                                .ToListAsync()
                                                .ConfigureAwait(false);

            var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider();
            await using (serviceProvider.ConfigureAwait(false))
            {
                var builder = serviceProvider.GetService<FractalLfgMessageBuilder>() ?? throw new InvalidOperationException();

                foreach (var configuration in configurations)
                {
                    // Refreshing of the lfg message
                    await builder.RefreshMessageAsync(configuration.Id).ConfigureAwait(false);
                }
            }
        }
    }

    #endregion // AsyncJob
}
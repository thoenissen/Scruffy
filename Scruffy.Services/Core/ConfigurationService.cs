using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Data.Enumerations.General;

namespace Scruffy.Services.Core;

/// <summary>
/// Configuration service
/// </summary>
public static class ConfigurationService
{
    #region Fields

    /// <summary>
    /// Maintenance mode
    /// </summary>
    private static bool? _isMaintenanceMode;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Maintenance mode
    /// </summary>
    public static bool IsMaintenanceMode => _isMaintenanceMode ??= Environment.GetEnvironmentVariable("SCRUFFY_MAINTENANCE_MODE") != "false";

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Gets an entry from the configuration
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Value</returns>
    public static string GetEntry(string key)
    {
        try
        {
            using (var repositoryFactory = RepositoryFactory.CreateInstance())
            {
                return repositoryFactory.GetRepository<CoreConfigurationRepository>()
                                        .GetQuery()
                                        .Where(entry => entry.Key == key)
                                        .Select(entry => entry.Value)
                                        .FirstOrDefault()
                           ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.CriticalError, nameof(ConfigurationService), "Failed to read configuration", key, ex);

            return string.Empty;
        }
    }

    #endregion // Methods
}
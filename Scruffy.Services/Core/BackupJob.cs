using Scruffy.Data.Entity;
using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Core;

/// <summary>
/// Backup of the sql database
/// </summary>
public class BackupJob : LocatedAsyncJob
{
    #region LocatedAsyncJob

    /// <inheritdoc/>
    public override async Task ExecuteOverrideAsync()
    {
        if (ConfigurationService.IsMaintenanceMode)
        {
            return;
        }

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            if (await dbFactory.ExecuteSqlRawAsync($"BACKUP DATABASE [{ConfigurationService.GetEntry("SCRUFFY_DB_CATALOG")}] TO  DISK = N'{ConfigurationService.GetEntry("SCRUFFY_DB_BACKUP_DIRECTORY")}{ConfigurationService.GetEntry("SCRUFFY_DB_CATALOG")}_{DateTime.Now:yyyyMMdd}.bak' WITH NOFORMAT, NOINIT,  NAME = N'{ConfigurationService.GetEntry("SCRUFFY_DB_CATALOG")}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD")
                               .ConfigureAwait(false) == null)
            {
                LoggingService.AddJobLogEntry(LogEntryLevel.CriticalError, nameof(BackupJob), "Database backup", dbFactory.LastError.Message, dbFactory.LastError.ToString());
            }
            else
            {
                if (await dbFactory.ExecuteSqlRawAsync($"BACKUP LOG [{ConfigurationService.GetEntry("SCRUFFY_DB_CATALOG")}] TO  DISK = N'{ConfigurationService.GetEntry("SCRUFFY_DB_BACKUP_DIRECTORY")}{ConfigurationService.GetEntry("SCRUFFY_DB_CATALOG")}_{DateTime.Now:yyyyMMdd}.trn' WITH NOFORMAT, NOINIT,  NAME = N'{ConfigurationService.GetEntry("SCRUFFY_DB_CATALOG")}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD")
                                   .ConfigureAwait(false) == null)
                {
                    LoggingService.AddJobLogEntry(LogEntryLevel.CriticalError, nameof(BackupJob), "Log backup", dbFactory.LastError.Message, dbFactory.LastError.ToString());
                }
            }
        }
    }

    #endregion // LocatedAsyncJob
}
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

    /// <summary>
    /// Executes the job
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task ExecuteOverrideAsync()
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            if (await dbFactory.ExecuteSqlRawAsync($"BACKUP DATABASE [{Environment.GetEnvironmentVariable("SCRUFFY_DB_CATALOG")}] TO  DISK = N'{Environment.GetEnvironmentVariable("SCRUFFY_DB_BACKUP_DIRECTORY")}{Environment.GetEnvironmentVariable("SCRUFFY_DB_CATALOG")}_{DateTime.Now:yyyyMMdd}.bak' WITH NOFORMAT, NOINIT,  NAME = N'{Environment.GetEnvironmentVariable("SCRUFFY_DB_CATALOG")}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD")
                               .ConfigureAwait(false) == null)
            {
                LoggingService.AddJobLogEntry(LogEntryLevel.CriticalError, nameof(BackupJob), "Database backup", dbFactory.LastError.Message, dbFactory.LastError.ToString());
            }
            else
            {
                if (await dbFactory.ExecuteSqlRawAsync($"BACKUP LOG [{Environment.GetEnvironmentVariable("SCRUFFY_DB_CATALOG")}] TO  DISK = N'{Environment.GetEnvironmentVariable("SCRUFFY_DB_BACKUP_DIRECTORY")}{Environment.GetEnvironmentVariable("SCRUFFY_DB_CATALOG")}_{DateTime.Now:yyyyMMdd}.trn' WITH NOFORMAT, NOINIT,  NAME = N'{Environment.GetEnvironmentVariable("SCRUFFY_DB_CATALOG")}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD")
                                   .ConfigureAwait(false) == null)
                {
                    LoggingService.AddJobLogEntry(LogEntryLevel.CriticalError, nameof(BackupJob), "Log backup", dbFactory.LastError.Message, dbFactory.LastError.ToString());
                }
            }
        }
    }

    #endregion // LocatedAsyncJob
}
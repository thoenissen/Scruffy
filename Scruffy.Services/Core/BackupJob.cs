using System;

using FluentScheduler;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.General;
using Scruffy.Data.Entity.Tables.General;
using Scruffy.Data.Enumerations.General;

namespace Scruffy.Services.Core
{
    /// <summary>
    /// Backup of the sql database
    /// </summary>
    public class BackupJob : IJob
    {
        #region IJob

        /// <summary>
        /// Executes the job.
        /// </summary>
        public void Execute()
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                if (dbFactory.ExecuteSqlCommand($"BACKUP DATABASE [{Environment.GetEnvironmentVariable("SCRUFFY_DB_CATALOG")}] TO  DISK = N'{Environment.GetEnvironmentVariable("SCRUFFY_DB_BACKUP_DIRECTORY")}{Environment.GetEnvironmentVariable("SCRUFFY_DB_CATALOG")}_{DateTime.Now:yyyyMMdd}.bak' WITH NOFORMAT, NOINIT,  NAME = N'{Environment.GetEnvironmentVariable("SCRUFFY_DB_CATALOG")}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD") == null)
                {
                    dbFactory.GetRepository<LogEntryRepository>()
                             .Add(new LogEntryEntity
                                  {
                                      TimeStamp = DateTime.Now,
                                      Type = LogEntryType.Job,
                                      Message = dbFactory.LastError.ToString(),
                                      QualifiedCommandName = nameof(BackupJob)
                                  });
                }
            }
        }

        #endregion // IJob
    }
}

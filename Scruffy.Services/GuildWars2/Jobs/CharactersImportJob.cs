using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Exceptions.WebApi;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.GuildWars2.Jobs;

/// <summary>
/// Import of Guild Wars 2 characters
/// </summary>
public class CharactersImportJob : LocatedAsyncJob
{
    #region Fields

    /// <summary>
    /// Factory
    /// </summary>
    private readonly RepositoryFactory _dbFactory;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbFactory">Factory</param>
    public CharactersImportJob(RepositoryFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    #endregion // Constructor

    #region LocatedAsyncJob

    /// <summary>
    /// Executes the job
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task ExecuteOverrideAsync()
    {
        foreach (var account in await _dbFactory.GetRepository<GuildWarsAccountRepository>()
                                                .GetQuery()
                                                .Where(obj => obj.Permissions.HasFlag(GuildWars2ApiPermission.RequiredPermissions))
                                                .Select(obj => new
                                                               {
                                                                   obj.Name,
                                                                   obj.ApiKey
                                                               })
                                                .ToListAsync()
                                                .ConfigureAwait(false))
        {
            try
            {
                var connector = new GuildWars2ApiConnector(account.ApiKey);
                await using (connector.ConfigureAwait(false))
                {
                    var characters = await connector.GetCharactersAsync()
                                                    .ConfigureAwait(false);

                    if (characters.Count > 0)
                    {
                        if (await _dbFactory.GetRepository<GuildWarsAccountHistoricCharacterRepository>()
                                            .BulkInsert(account.Name, characters)
                                            .ConfigureAwait(false) == false)
                        {
                            LoggingService.AddJobLogEntry(LogEntryLevel.Error,
                                                          nameof(CharactersImportJob),
                                                          $"Unknown error while importing account ({account}) characters",
                                                          null,
                                                          _dbFactory.LastError);
                        }
                    }
                }
            }
            catch (MissingGuildWars2ApiPermissionException ex)
            {
                LoggingService.AddJobLogEntry(LogEntryLevel.Error,
                                              nameof(CharactersImportJob),
                                              $"Missing permissions {account}",
                                              null,
                                              ex);
            }
            catch (Exception ex)
            {
                LoggingService.AddJobLogEntry(LogEntryLevel.Error,
                                              nameof(CharactersImportJob),
                                              $"Unknown error with account {account}",
                                              null,
                                              ex);
            }
        }
    }

    #endregion // LocatedAsyncJob
}
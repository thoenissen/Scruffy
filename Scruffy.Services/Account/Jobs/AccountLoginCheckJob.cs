﻿using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Account;
using Scruffy.Data.Entity.Tables.GuildWars2.Account;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Account.Jobs;

/// <summary>
/// Checking the daily account login
/// </summary>
public class AccountLoginCheckJob : LocatedAsyncJob
{
    #region LocatedAsyncJob

    /// <inheritdoc/>
    public override async Task ExecuteOverrideAsync()
    {
        var date = DateTime.Today.AddDays(-1);

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            foreach (var account in dbFactory.GetRepository<AccountRepository>()
                                             .GetQuery()
                                             .Where(obj => obj.Permissions.HasFlag(GuildWars2ApiPermission.RequiredPermissions))
                                             .Select(obj => new
                                                            {
                                                                obj.Name,
                                                                obj.ApiKey,
                                                                obj.LastAge,
                                                                WordId = obj.WorldId
                                                            })
                                             .ToList())
            {
                try
                {
                    var connector = new GuildWars2ApiConnector(account.ApiKey);

                    await using (connector.ConfigureAwait(false))
                    {
                        var accountInformation = await connector.GetAccountInformationAsync()
                                                                .ConfigureAwait(false);

                        if (accountInformation.Age != account.LastAge)
                        {
                            dbFactory.GetRepository<AccountDailyLoginCheckRepository>()
                                     .Add(new GuildWarsAccountDailyLoginCheckEntity
                                          {
                                              Name = account.Name,
                                              Date = date
                                          });

                            dbFactory.GetRepository<AccountRepository>()
                                     .Refresh(obj => obj.Name == account.Name,
                                              obj =>
                                              {
                                                  obj.LastAge = accountInformation.Age;
                                                  obj.WorldId = accountInformation.World;
                                                  obj.DailyAchievementPoints = accountInformation.DailyAchievementPoints;
                                                  obj.MonthlyAchievementPoints = accountInformation.MonthlyAchievementPoints;
                                              });
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.AddJobLogEntry(LogEntryLevel.CriticalError, nameof(AccountLoginCheckJob), account.Name, ex.Message, ex.ToString());
                }
            }
        }
    }

    #endregion // LocatedAsyncJob
}
using System;
using System.Linq;
using System.Threading.Tasks;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Account;
using Scruffy.Data.Entity.Repositories.General;
using Scruffy.Data.Entity.Tables.Account;
using Scruffy.Data.Entity.Tables.General;
using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Account
{
    /// <summary>
    /// Checking the daily account login
    /// </summary>
    public class AccountLoginCheckJob : LocatedAsyncJob
    {
        #region LocatedAsyncJob

        /// <summary>
        /// Executes the job
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task ExecuteAsync()
        {
            var date = DateTime.Today.AddDays(-1);

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                foreach (var account in dbFactory.GetRepository<AccountRepository>()
                                                 .GetQuery()
                                                 .Select(obj => new
                                                               {
                                                                   obj.Name,
                                                                   obj.ApiKey,
                                                                   obj.LastAge,
                                                                   obj.WordId
                                                               })
                                                 .ToList())
                {
                    try
                    {
                        await using (var connector = new GuidWars2ApiConnector(account.ApiKey))
                        {
                            var accountInformation = await connector.GetAccountInformationAsync()
                                                                    .ConfigureAwait(false);

                            if (accountInformation.Age != account.LastAge)
                            {
                                dbFactory.GetRepository<AccountDailyLoginCheckRepository>()
                                         .Add(new AccountDailyLoginCheckEntity
                                              {
                                                  Name = account.Name,
                                                  Date = date
                                              });

                                dbFactory.GetRepository<AccountRepository>()
                                         .Refresh(obj => obj.Name == account.Name,
                                                  obj =>
                                                  {
                                                      obj.LastAge = accountInformation.Age;
                                                      obj.WordId = accountInformation.World;
                                                  });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        dbFactory.GetRepository<LogEntryRepository>()
                                 .Add(new LogEntryEntity
                                          {
                                              TimeStamp = DateTime.Now,
                                              Type = LogEntryType.Job,
                                              QualifiedCommandName = nameof(AccountLoginCheckJob),
                                              LastUserCommand = account.Name,
                                              Message = ex.ToString()
                                          });
                    }
                }
            }
        }

        #endregion // LocatedAsyncJob
    }
}

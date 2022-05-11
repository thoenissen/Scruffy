using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.GameData;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.GuildWars2;

/// <summary>
/// Achievement service
/// </summary>
public class AchievementService : LocatedServiceBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public AchievementService(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Importing all achievements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<bool> ImportAchievements()
    {
        var connector = new GuildWars2ApiConnector(null);
        await using (connector.ConfigureAwait(false))
        {
            var achievementIds = await connector.GetAllAchievementIds()
                                                .ConfigureAwait(false);

            var achievements = await connector.GetAchievements(achievementIds)
                                              .ConfigureAwait(false);

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                return await dbFactory.GetRepository<GuildWarsAchievementRepository>()
                                      .BulkInsert(achievements)
                                      .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Import all achievement of an user
    /// </summary>
    /// <param name="accountName">Account name</param>
    /// <param name="apiKey">API-Key</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<bool> ImportAccountAchievements(string accountName, string apiKey)
    {
        var connector = new GuildWars2ApiConnector(apiKey);
        await using (connector.ConfigureAwait(false))
        {
            var achievements = await connector.GetAccountAchievements()
                                              .ConfigureAwait(false);

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                return await dbFactory.GetRepository<GuildWarsAccountAchievementRepository>()
                                      .BulkInsert(accountName, achievements)
                                      .ConfigureAwait(false);
            }
        }
    }

    #endregion // Methods
}
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Guild;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Guild
{
    /// <summary>
    /// Guild rank management
    /// </summary>
    public class GuildRankService : LocatedServiceBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public GuildRankService(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Import all in game ranks
        /// </summary>
        /// <param name="guildId">Id of the guild</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ImportGuildRanks(long? guildId)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                foreach (var guild in dbFactory.GetRepository<GuildRepository>()
                                               .GetQuery()
                                               .Where(obj => guildId == null
                                                          || obj.Id == guildId)
                                               .Select(obj => new
                                                              {
                                                                  obj.Id,
                                                                  obj.GuildId,
                                                                  obj.ApiKey
                                                              })
                                               .ToList())
                {
                    var connector = new GuidWars2ApiConnector(guild.ApiKey);
                    await using (connector.ConfigureAwait(false))
                    {
                        var members = await connector.GetGuildMembers(guild.GuildId)
                                                     .ConfigureAwait(false);

                        await dbFactory.GetRepository<GuildWarsGuildMemberRepository>()
                                       .BulkInsert(guild.Id, members.Select(obj => (obj.Name, obj.Rank )))
                                       .ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Refreshing the member rank
        /// </summary>
        /// <param name="guildId">Id of the guild</param>
        /// <param name="accountName">Account name</param>
        /// <param name="rankName">rank</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RefreshDiscordRank(long guildId, string accountName, string rankName)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                dbFactory.GetRepository<GuildWarsGuildMemberRepository>()
                         .AddOrRefresh(obj => obj.GuildId == guildId
                                           && obj.Name == accountName,
                                       obj =>
                                       {
                                           obj.GuildId = guildId;
                                           obj.Name = accountName;
                                           obj.Rank = rankName;
                                       });
            }
        }

        #endregion // Methods
    }
}

using Discord;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.Guild;
using Scruffy.Data.Json.QuickChart;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Extensions;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord.Interfaces;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Guild
{
    /// <summary>
    /// Guild rank visualization service
    /// </summary>
    public class GuildRankVisualizationService : LocatedServiceBase
    {
        #region Fields

        /// <summary>
        /// Repository factory
        /// </summary>
        private readonly RepositoryFactory _dbFactory;

        /// <summary>
        /// QuickChart.io connector
        /// </summary>
        private readonly QuickChartConnector _quickChartConnector;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="dbFactory">Repository factory</param>
        /// <param name="quickChartConnector">QuickChart.io connector</param>
        public GuildRankVisualizationService(LocalizationService localizationService, RepositoryFactory dbFactory, QuickChartConnector quickChartConnector)
            : base(localizationService)
        {
            _dbFactory = dbFactory;
            _quickChartConnector = quickChartConnector;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Post overview
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PostOverview(IContextContainer context)
        {
            var limit = DateTime.Today.AddDays(-60);

            var guildMemberSubQuery = _dbFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                                .GetQuery()
                                                .Select(obj => obj);

            var guildMemberQuery = _dbFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                             .GetQuery()
                                             .Where(obj => guildMemberSubQuery.Any(obj2 => obj2.GuildId == obj.GuildId
                                                                                        && obj2.Date > obj.Date) == false);
            var accountsQuery = _dbFactory.GetRepository<GuildWarsAccountRepository>()
                                          .GetQuery()
                                          .Select(obj => obj);

            var discordUsersQuery = _dbFactory.GetRepository<DiscordAccountRepository>()
                                              .GetQuery()
                                              .Select(obj => obj);

            var userPoints = _dbFactory.GetRepository<GuildRankCurrentPointsRepository>()
                                       .GetQuery()
                                       .Where(obj => obj.Date >= limit
                                                  && obj.Guild.DiscordServerId == context.Guild.Id
                                                  && accountsQuery.Any(obj2 => obj2.UserId == obj.UserId
                                                                            && guildMemberQuery.Any(obj3 => obj3.Name == obj2.Name
                                                                                                         && obj3.GuildId == obj.GuildId)))
                                       .GroupBy(obj => obj.UserId)
                                       .Select(obj => new
                                                      {
                                                          UserId = obj.Key,
                                                          Points = obj.Sum(obj2 => obj2.Points),
                                                          DiscordUserId = discordUsersQuery.Where(obj2 => obj2.UserId == obj.Key)
                                                                                           .Select(obj2 => (ulong?)obj2.Id)
                                                                                           .FirstOrDefault()
                                                      })
                                       .ToList();

            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle(LocalizationGroup.GetText("RankingOverview", "Guild ranking points overview"));
            embedBuilder.WithColor(Color.DarkBlue);
            embedBuilder.WithImageUrl("attachment://chart.png");

            var userNames = new List<string>();

            foreach (var user in userPoints)
            {
                if (user.DiscordUserId != null)
                {
                    var member = await context.Guild
                                              .GetUserAsync(user.DiscordUserId.Value)
                                              .ConfigureAwait(false);

                    userNames.Add($"{member.TryGetDisplayName()} [{user.Points:0.00}]");
                }
            }

            var chartConfiguration = new ChartConfigurationData
                                     {
                                         Type = "horizontalBar",
                                         Data = new Data.Json.QuickChart.Data
                                                {
                                                    DataSets = new List<DataSet>
                                                               {
                                                                   new DataSet<double>
                                                                   {
                                                                       BackgroundColor = userPoints.Select(obj => "#316ed5")
                                                                                                   .ToList(),
                                                                       BorderColor = "#274d85",
                                                                       Data = userPoints.Select(obj => obj.Points)
                                                                                        .ToList()
                                                                   }
                                                               },
                                                    Labels = userNames
                                                },
                                         Options = new OptionsCollection
                                                   {
                                                       Scales = new ScalesCollection
                                                                {
                                                                    XAxes = new List<XAxis>
                                                                            {
                                                                                new ()
                                                                                {
                                                                                    Ticks = new AxisTicks<double>
                                                                                            {
                                                                                                MinValue = 0,
                                                                                                MaxValue = Math.Ceiling(((userPoints.Max(obj => obj.Points) / 10) + 1) * 10),
                                                                                                FontColor = "#b3b3b3"
                                                                                            }
                                                                                }
                                                                            },
                                                                    YAxes = new List<YAxis>
                                                                            {
                                                                                new ()
                                                                                {
                                                                                    Ticks = new AxisTicks<double>
                                                                                            {
                                                                                                FontColor = "#b3b3b3"
                                                                                            }
                                                                                }
                                                                            }
                                                                },
                                                       Plugins = new PluginsCollection
                                                                 {
                                                                     Legend = false
                                                                 }
                                                   }
                                     };

            var chartStream = await _quickChartConnector.GetChartAsStream(new ChartData
                                                                          {
                                                                              Width = 500,
                                                                              Height = 20 * userNames.Count,
                                                                              DevicePixelRatio = 1,
                                                                              BackgroundColor = "#262626",
                                                                              Format = "png",
                                                                              Config = JsonConvert.SerializeObject(chartConfiguration,
                                                                                                                   new JsonSerializerSettings
                                                                                                                   {
                                                                                                                       NullValueHandling = NullValueHandling.Ignore
                                                                                                                   })
                                                                          })
                                                        .ConfigureAwait(false);

            await using (chartStream.ConfigureAwait(false))
            {
                await context.Channel
                             .SendFileAsync(new FileAttachment(chartStream, "chart.png"), embed: embedBuilder.Build())
                             .ConfigureAwait(false);
            }
        }

        #endregion // Methods
    }
}

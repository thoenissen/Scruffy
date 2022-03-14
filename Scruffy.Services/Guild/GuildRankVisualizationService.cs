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
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord.Interfaces;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Guild;

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

    /// <summary>
    /// User management service
    /// </summary>
    private readonly UserManagementService _userManagementService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="dbFactory">Repository factory</param>
    /// <param name="quickChartConnector">QuickChart.io connector</param>
    /// <param name="userManagementService">User management service</param>
    public GuildRankVisualizationService(LocalizationService localizationService,
                                         RepositoryFactory dbFactory,
                                         QuickChartConnector quickChartConnector,
                                         UserManagementService userManagementService)
        : base(localizationService)
    {
        _dbFactory = dbFactory;
        _quickChartConnector = quickChartConnector;
        _userManagementService = userManagementService;
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
        var limit = DateTime.Today.AddDays(-64);

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
                                   .OrderByDescending(obj => obj.Points)
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
                if (member != null)
                {
                    userNames.Add($"{member.TryGetDisplayName()} [{user.Points:0.00}]");
                }
            }
        }

        var minValue = userPoints.Min(obj => obj.Points);

        minValue = minValue > 0
                       ? 0
                       : -10 * (((int)Math.Ceiling(minValue * -1) / 10) + 1);

        var maxValue = (((int)Math.Ceiling(userPoints.Max(obj => obj.Points)) / 10) + 1) * 10;

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
                                                                                            MinValue = minValue,
                                                                                            MaxValue = maxValue,
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

    /// <summary>
    /// Post personal overview
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="guildUser">User</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PostPersonalOverview(IContextContainer context, IGuildUser guildUser)
    {
        var user = await _userManagementService.GetUserByDiscordAccountId(guildUser.Id)
                                               .ConfigureAwait(false);

        var limit = DateTime.Today.AddDays(-64);

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

        var userPoints = _dbFactory.GetRepository<GuildRankCurrentPointsRepository>()
                                   .GetQuery()
                                   .Where(obj => obj.Date >= limit
                                              && obj.Guild.DiscordServerId == context.Guild.Id
                                              && obj.UserId == user.Id
                                              && accountsQuery.Any(obj2 => obj2.UserId == obj.UserId
                                                                        && guildMemberQuery.Any(obj3 => obj3.Name == obj2.Name
                                                                                                     && obj3.GuildId == obj.GuildId)))
                                   .GroupBy(obj => obj.Type)
                                   .Select(obj => new
                                                  {
                                                      Type = obj.Key,
                                                      Points = obj.Sum(obj2 => obj2.Points)
                                                  })
                                   .OrderByDescending(obj => obj.Points)
                                   .ToList();

        var embedBuilder = new EmbedBuilder();
        embedBuilder.WithTitle($"{LocalizationGroup.GetText("RankingPersonalOverview", "Guild ranking personal points overview")} ({guildUser.TryGetDisplayName()})");
        embedBuilder.WithColor(Color.DarkBlue);
        embedBuilder.WithImageUrl("attachment://chart.png");

        var chartConfiguration = new ChartConfigurationData
                                 {
                                     Type = "bar",
                                     Data = new Data.Json.QuickChart.Data
                                            {
                                                DataSets = new List<DataSet>
                                                           {
                                                               new DataSet<double>
                                                               {
                                                                   BorderColor = "#333333",
                                                                   BackgroundColor = new List<string>
                                                                                     {
                                                                                         "#0d1c26",
                                                                                         "#142b39",
                                                                                         "#1d3e53",
                                                                                         "#21475e",
                                                                                         "#2e6384",
                                                                                         "#357197",
                                                                                         "#3c80aa",
                                                                                         "#428ebd",
                                                                                         "#5599c3",
                                                                                         "#68a4ca"
                                                                                     },
                                                                   Data = userPoints.Select(obj => obj.Points)
                                                                                    .ToList()
                                                               }
                                                           },
                                                Labels = userPoints.Select(obj => $"{LocalizationGroup.GetText(obj.Type.ToString(), obj.Type.ToString())} ({obj.Points.ToString("0.##", LocalizationGroup.CultureInfo)})")
                                                                   .ToList()
                                            },
                                     Options = new OptionsCollection
                                               {
                                                   Plugins = new PluginsCollection
                                                             {
                                                                 Legend = false,
                                                                 OutLabels = new OutLabelsCollection
                                                                             {
                                                                                 Text = "%l",
                                                                                 Stretch = 40
                                                                             },
                                                                 DoughnutLabel = new DoughnutLabelCollection
                                                                                 {
                                                                                     Labels = new List<Label>
                                                                                              {
                                                                                                  new ()
                                                                                                  {
                                                                                                      Color = "white",
                                                                                                      Text = userPoints.Sum(obj => obj.Points).ToString("0.##", LocalizationGroup.CultureInfo)
                                                                                                  },
                                                                                                  new ()
                                                                                                  {
                                                                                                      Color = "white",
                                                                                                      Text = LocalizationGroup.GetText("MeOverviewPoints", "points")
                                                                                                  },
                                                                                              }
                                                                                 },
                                                             },
                                                   Title = new TitleConfiguration
                                                           {
                                                               Display = true,
                                                               FontColor = "white",
                                                               FontSize = 30,
                                                               Text = LocalizationGroup.GetText("MeOverviewChartTitle", "Point distribution")
                                                           }
                                               }
                                 };

        var chartStream = await _quickChartConnector.GetChartAsStream(new ChartData
                                                                      {
                                                                          Width = 600,
                                                                          Height = 500,
                                                                          BackgroundColor = "#2f3136",
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
            embedBuilder.WithImageUrl("attachment://chart.png");

            await context.Channel
                         .SendFileAsync(new FileAttachment(chartStream, "chart.png"),
                                        embed: embedBuilder.Build())
                         .ConfigureAwait(false);
        }
    }

    #endregion // Methods
}
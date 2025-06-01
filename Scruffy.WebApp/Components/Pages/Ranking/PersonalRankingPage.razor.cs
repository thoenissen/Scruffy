using System;
using System.Linq;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.Guild;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Data.Extensions;
using Scruffy.Data.Json.ChartJs;
using Scruffy.Data.Services.Guild;
using Scruffy.Services.Core.Localization;

namespace Scruffy.WebApp.Components.Pages.Ranking;

/// <summary>
/// Personal ranking page
/// </summary>
[Authorize(Roles = "Member")]
public partial class PersonalRankingPage
{
    #region Fields

    /// <summary>
    /// Distribution chart options
    /// </summary>
    private readonly ChartOptions _distributionChartOptions;

    /// <summary>
    /// History chart options
    /// </summary>
    private readonly ChartOptions _historyChartOptions;

    /// <summary>
    /// Distribution chart data
    /// </summary>
    private ChartData _distributionChartData;

    /// <summary>
    /// History chart data
    /// </summary>
    private ChartData _historyChartData;

    /// <summary>
    /// Type descriptions
    /// </summary>
    private LocalizationGroup _typeDescriptions;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public PersonalRankingPage()
    {
        _distributionChartOptions = new ChartOptions
                                    {
                                        Responsive = true,
                                        MaintainAspectRatio = false,
                                        Plugins = new PluginsCollection
                                                  {
                                                      Legend = false
                                                  },
                                        Scales = new Scales
                                                 {
                                                     X = new Axes
                                                         {
                                                             Grid = new GridConfiguration(),
                                                             Ticks = new AxisTicks
                                                                     {
                                                                         Color = "#A0AEC0"
                                                                     }
                                                         },
                                                     Y = new Axes
                                                         {
                                                             Grid = new GridConfiguration
                                                                    {
                                                                        Color = "lightgrey"
                                                                    },
                                                             Ticks = new AxisTicks
                                                                     {
                                                                         Color = "#A0AEC0"
                                                                     }
                                                         }
                                                 }
                                    };
        _historyChartOptions = new ChartOptions
                               {
                                   Responsive = true,
                                   MaintainAspectRatio = false,
                                   Plugins = new PluginsCollection
                                             {
                                                 Legend = true
                                             },
                                   Scales = new Scales
                                            {
                                                X = new Axes
                                                    {
                                                        Grid = new GridConfiguration(),
                                                        Ticks = new AxisTicks
                                                                {
                                                                    Color = "#A0AEC0"
                                                                }
                                                    },
                                                Y = new Axes
                                                    {
                                                        Stacked = true,
                                                        Grid = new GridConfiguration
                                                               {
                                                                   Color = "lightgrey"
                                                               },
                                                        Ticks = new AxisTicks
                                                                {
                                                                    Color = "#A0AEC0"
                                                                }
                                                    }
                                            }
                               };
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Authentication state provider
    /// </summary>
    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Build distribution chart
    /// </summary>
    /// <param name="userId">User ID</param>
    private void BuildDistributionChart(int userId)
    {
        var limit = DateTime.Today.AddDays(-63);
        var today = DateTime.Today;

        using (var repositoryFactory = RepositoryFactory.CreateInstance())
        {
            var guildMemberSubQuery = repositoryFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                                       .GetQuery();

            var guildMemberQuery = repositoryFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                                    .GetQuery()
                                                    .Where(member => guildMemberSubQuery.Any(checkMember => checkMember.GuildId == member.GuildId
                                                                     && checkMember.Date > member.Date) == false);
            var accountsQuery = repositoryFactory.GetRepository<GuildWarsAccountRepository>()
                                                 .GetQuery();

            var userConfiguration = repositoryFactory.GetRepository<GuildUserConfigurationRepository>()
                                                     .GetQuery();

            var userPoints = repositoryFactory.GetRepository<GuildRankCurrentPointsRepository>()
                                              .GetQuery()
                                              .Where(rankPoint => rankPoint.Date >= limit
                                                                  && rankPoint.Date < today
                                                                  && rankPoint.Guild.DiscordServerId == WebAppConfiguration.DiscordServerId
                                                                  && rankPoint.UserId == userId
                                                                  && accountsQuery.Any(account => account.UserId == rankPoint.UserId
                                                                                                  && guildMemberQuery.Any(member => member.Name == account.Name
                                                                                                      && member.GuildId == rankPoint.GuildId))
                                                                  && userConfiguration.Any(configuration => configuration.UserId == rankPoint.UserId
                                                                                                            && configuration.GuildId == rankPoint.GuildId
                                                                                                            && configuration.IsInactive) == false)
                                              .GroupBy(rankPoint => rankPoint.Type)
                                              .Select(rankPoint => new GuildRankUserPointsData
                                                                   {
                                                                       Type = rankPoint.Key,
                                                                       Points = rankPoint.Sum(obj2 => obj2.Points)
                                                                   })
                                              .Where(rankPoint => rankPoint.Points != 0)
                                              .OrderByDescending(rankPoint => rankPoint.Points)
                                              .ToList();

            if (userPoints.Count > 0)
            {
                _distributionChartData = new ChartData
                                         {
                                             Datasets = [
                                                            new DataSet
                                                            {
                                                                Data = userPoints.Select(obj => obj.Points)
                                                                                 .ToArray(),
                                                                BackgroundColor = userPoints.Select(obj => obj.Type.GetColor())
                                                                                            .ToArray()
                                                            }
                                                        ],
                                             Labels = userPoints.Select(obj => $"{GetDescription(obj.Type)} ({obj.Points:0.##})").ToArray(),
                                         };
            }
        }
    }

    /// <summary>
    /// Build history chart
    /// </summary>
    /// <param name="userId">User ID</param>
    private void BuildHistoryChart(int userId)
    {
        using (var repositoryFactory = RepositoryFactory.CreateInstance())
        {
            var from = DateTime.Today.AddDays(-63);
            var to = DateTime.Today.AddDays(-1);

            var userConfiguration = repositoryFactory.GetRepository<GuildUserConfigurationRepository>()
                                                     .GetQuery();

            var currentPoints = repositoryFactory.GetRepository<GuildRankCurrentPointsRepository>()
                                                 .GetQuery()
                                                 .Where(rankPoint => rankPoint.UserId == userId
                                                                     && rankPoint.Date >= from
                                                                     && rankPoint.Date <= to
                                                                     && rankPoint.Points != 0
                                                                     && rankPoint.Guild.DiscordServerId == WebAppConfiguration.DiscordServerId
                                                                     && userConfiguration.Any(configuration => configuration.UserId == rankPoint.UserId
                                                                                                               && configuration.GuildId == rankPoint.GuildId
                                                                                                               && configuration.IsInactive) == false)
                                                 .Select(rankPoint => new
                                                         {
                                                             rankPoint.Date,
                                                             rankPoint.Type,
                                                             rankPoint.Points
                                                         })
                                                 .ToList();

            if (currentPoints.Count > 0)
            {
                var types = Enum.GetValues<GuildRankPointType>();
                var dataSets = new DataSet[types.Length];
                var dates = Enumerable.Range(-63, 63)
                                      .Select(obj => DateTime.Today.AddDays(obj))
                                      .ToList();

                for (var index = 0; index < types.Length; index++)
                {
                    var type = types[index];

                    dataSets[index] = new DataSet
                                      {
                                          Label = GetDescription(type),
                                          Data = dates.Select(date => currentPoints.FirstOrDefault(rankPoint => rankPoint.Date == date
                                                                                                                && rankPoint.Type == type)?.Points
                                                                          ?? 0.0)
                                                      .ToArray(),
                                          BackgroundColor = [type.GetColor()],
                                          BorderColor = [type.GetColor()]
                                      };
                }

                _historyChartData = new ChartData
                                    {
                                        Datasets = dataSets,
                                        Labels = dates.Select(obj => obj.ToString("dd.MM.yyyy"))
                                                      .ToArray().ToArray()
                                    };
            }
        }
    }

    /// <summary>
    /// Get description
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns>Description</returns>
    private string GetDescription(GuildRankPointType type)
    {
        _typeDescriptions ??= LocalizationService.GetGroup(nameof(GuildRankPointType));

        return _typeDescriptions.GetText(type.ToString(), type.ToString());
    }

    #endregion // Methods

    #region ComponentBase

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        var authState = AuthenticationStateProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();

        var nameIdentifier = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(nameIdentifier) == false
            && int.TryParse(nameIdentifier, out var userId))
        {
            BuildDistributionChart(userId);
            BuildHistoryChart(userId);
        }
    }

    #endregion // ComponentBase
}
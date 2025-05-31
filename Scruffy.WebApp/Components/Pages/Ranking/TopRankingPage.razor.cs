using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Hybrid;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Json.ChartJs;
using Scruffy.WebApp.Components.Base;
using Scruffy.WebApp.Components.Controls;
using Scruffy.WebApp.Components.Pages.Ranking.Data;

namespace Scruffy.WebApp.Components.Pages.Ranking;

/// <summary>
/// Max points page
/// </summary>
[Authorize(Roles = "Member")]
public partial class TopRankingPage : LocatedComponent
{
    #region Fields

    /// <summary>
    /// Max points chart options
    /// </summary>
    private readonly ChartOptions _maxPointsChartOptions;

    /// <summary>
    /// Is max points being loading?
    /// </summary>
    private bool _isMaxPointsLoading;

    /// <summary>
    /// Max points chart
    /// </summary>
    private Chart _maxPointsChart;

    /// <summary>
    /// Max points chart data
    /// </summary>
    private ChartData _maxPointsChartData;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public TopRankingPage()
    {
        _maxPointsChartOptions = new ChartOptions
                                 {
                                     Responsive = true,
                                     MaintainAspectRatio = false,
                                     Plugins = new PluginsCollection
                                               {
                                                   Legend = false
                                               },
                                     IndexAxis = "y",
                                     Scales = new Scales
                                              {
                                                  X = new Axes
                                                      {
                                                          Grid = new GridConfiguration
                                                                 {
                                                                     Color = "lightgrey"
                                                                 },
                                                          Ticks = new AxisTicks
                                                                  {
                                                                      Color = "#A0AEC0"
                                                                  }
                                                      },
                                                  Y = new Axes
                                                      {
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
    /// Cache
    /// </summary>
    [Inject]
    private HybridCache Cache { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Build max points chart
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task BuildMaxPointsChart()
    {
        var maxPoints = await Cache.GetOrCreateAsync("webapp/ranking/top/max_points",
                                                     _ => ValueTask.FromResult(GetMaxPointsEntries()),
                                                     new HybridCacheEntryOptions
                                                     {
                                                         Expiration = TimeSpan.FromHours(24)
                                                     })
                                   .ConfigureAwait(false);

        if (maxPoints.Count > 0)
        {
            _maxPointsChartData = new ChartData
                                  {
                                      Datasets = [
                                                     new DataSet
                                                     {
                                                         Data = maxPoints.Select(points => points.Points)
                                                                         .ToArray(),
                                                         BackgroundColor = maxPoints.Select(_ => "#316ed5")
                                                                                    .ToArray()
                                                     }
                                                 ],
                                      Labels = maxPoints.Select(topPoints => $"{topPoints.Name} ({topPoints.Points:0.##})")
                                                        .ToArray(),
                                  };
        }

        _isMaxPointsLoading = false;

        await InvokeAsync(async () =>
                          {
                              if (_maxPointsChart != null)
                              {
                                  await _maxPointsChart.Update()
                                      .ConfigureAwait(true);
                              }

                              StateHasChanged();
                          }).ConfigureAwait(false);
    }

    /// <summary>
    /// Get max points data
    /// </summary>
    /// <returns>Liste of entries</returns>
    private List<MaxPointsEntry> GetMaxPointsEntries()
    {
        using (var repositoryFactory = RepositoryFactory.CreateInstance())
        {
            var currentPointsQuery = repositoryFactory.GetRepository<GuildRankCurrentPointsRepository>()
                                                 .GetQuery();

            var discordMembers = repositoryFactory.GetRepository<DiscordServerMemberRepository>()
                                                   .GetQuery();

            return repositoryFactory.GetRepository<GuildRankCurrentPointsRepository>()
                                    .GetQuery()
                                    .Where(currentPoints => currentPoints.Guild.DiscordServerId == WebAppConfiguration.DiscordServerId)
                                    .Select(currentPoints => new
                                                             {
                                                                 currentPoints.GuildId,
                                                                 currentPoints.UserId,
                                                                 currentPoints.Date
                                                             })
                                    .Distinct()
                                    .Select(dates => new
                                                     {
                                                         dates.UserId,
                                                         dates.Date,
                                                         Points = currentPointsQuery.Where(points => points.GuildId == dates.GuildId
                                                                                                     && points.UserId == dates.UserId
                                                                                                     && points.Date > dates.Date.AddDays(-63)
                                                                                                     && points.Date <= dates.Date)
                                                                                    .Sum(points => points.Points),
                                                     })
                                    .GroupBy(dates => dates.UserId)
                                    .Select(pointsPerUser => new MaxPointsEntry
                                                             {
                                                                 Name = discordMembers.Where(member => member.Account.UserId == pointsPerUser.Key
                                                                                                       && member.ServerId == WebAppConfiguration.DiscordServerId)
                                                                                      .Select(member => member.Name)
                                                                                      .FirstOrDefault(),
                                                                 Points = pointsPerUser.Max(points => points.Points),
                                                             })
                                    .Where(pointsPerUser => pointsPerUser.Name != null)
                                    .OrderByDescending(pointsPerUser => pointsPerUser.Points)
                                    .Take(40)
                                    .ToList();
        }
    }

    #endregion // Methods

    #region ComponentBase

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (_isMaxPointsLoading == false)
        {
            _isMaxPointsLoading = true;

            Task.Run(BuildMaxPointsChart);
        }
    }

    #endregion // ComponentBase
}
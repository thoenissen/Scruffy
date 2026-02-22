using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Statistics;
using Scruffy.Data.Json.ChartJs;
using Scruffy.WebApp.Components.Base;
using Scruffy.WebApp.Components.Controls;

namespace Scruffy.WebApp.Components.Pages.Statistics;

/// <summary>
/// Discord message statistics page
/// </summary>
[Authorize(Roles = "Member")]
public partial class DiscordMessageStatisticsPage : LocatedComponent
{
    #region Nested types

    /// <summary>
    /// Time filter options
    /// </summary>
    private enum TimeFilter
    {
        /// <summary>
        /// Last 30 days
        /// </summary>
        Days30,

        /// <summary>
        /// Last 90 days
        /// </summary>
        Days90,

        /// <summary>
        /// Last 180 days
        /// </summary>
        Days180,

        /// <summary>
        /// Last year
        /// </summary>
        Year1,

        /// <summary>
        /// All time
        /// </summary>
        All
    }

    #endregion // Nested types

    #region Constants

    /// <summary>
    /// Number of top entries to display individually
    /// </summary>
    private const int TopCount = 10;

    /// <summary>
    /// Pie chart color palette
    /// </summary>
    private static readonly string[] _pieColors = ["#f1d083", "#7b68ee", "#ff6384", "#36a2eb", "#4bc0c0", "#9966ff", "#ff9f40", "#c9cbcf", "#e7e9ed", "#8ac926", "#6a4c93"];

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Overview chart options
    /// </summary>
    private readonly ChartOptions _overviewChartOptions;

    /// <summary>
    /// Pie chart options
    /// </summary>
    private readonly ChartOptions _pieChartOptions;

    /// <summary>
    /// Currently selected filter
    /// </summary>
    private TimeFilter _selectedFilter = TimeFilter.Days30;

    /// <summary>
    /// Is data being loaded?
    /// </summary>
    private bool _isLoading;

    /// <summary>
    /// Overview chart data
    /// </summary>
    private ChartData _overviewChartData;

    /// <summary>
    /// Top users chart data
    /// </summary>
    private ChartData _topUsersChartData;

    /// <summary>
    /// Top channels chart data
    /// </summary>
    private ChartData _topChannelsChartData;

    /// <summary>
    /// All users table data
    /// </summary>
    private List<(string Name, int Count)> _allUsersTableData;

    /// <summary>
    /// All channels table data
    /// </summary>
    private List<(string Name, int Count)> _allChannelsTableData;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public DiscordMessageStatisticsPage()
    {
        _overviewChartOptions = new ChartOptions
                                {
                                    Responsive = true,
                                    MaintainAspectRatio = false,
                                    Plugins = new PluginsCollection
                                              {
                                                  Legend = new LegendPlugin
                                                           {
                                                               Display = true,
                                                               Labels = new LegendLabels
                                                                        {
                                                                            Color = WebAppConfiguration.Colors.Text
                                                                        }
                                                           }
                                              },
                                    Scales = new Scales
                                             {
                                                 X = new Axes
                                                     {
                                                         Grid = new GridConfiguration(),
                                                         Ticks = new AxisTicks
                                                                 {
                                                                     Color = WebAppConfiguration.Colors.Text
                                                                 }
                                                     },
                                                 Y = new Axes
                                                     {
                                                         Grid = new GridConfiguration
                                                                {
                                                                    Color = WebAppConfiguration.Colors.Text
                                                                },
                                                         Ticks = new AxisTicks
                                                                 {
                                                                     Color = WebAppConfiguration.Colors.Text
                                                                 }
                                                     }
                                             }
                                };

        _pieChartOptions = new ChartOptions
                           {
                               Responsive = true,
                               MaintainAspectRatio = false,
                               Plugins = new PluginsCollection
                                         {
                                             Legend = new LegendPlugin
                                                      {
                                                          Display = true,
                                                          Position = "right",
                                                          Labels = new LegendLabels
                                                                   {
                                                                       Color = WebAppConfiguration.Colors.Text
                                                                   }
                                                      }
                                         }
                           };
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Get the cutoff date for the selected filter
    /// </summary>
    /// <param name="filter">Time filter</param>
    /// <returns>Cutoff date or null for all time</returns>
    private static DateTime? GetCutoffDate(TimeFilter filter)
    {
        return filter switch
               {
                   TimeFilter.Days30 => DateTime.UtcNow.AddDays(-30),
                   TimeFilter.Days90 => DateTime.UtcNow.AddDays(-90),
                   TimeFilter.Days180 => DateTime.UtcNow.AddDays(-180),
                   TimeFilter.Year1 => DateTime.UtcNow.AddYears(-1),
                   _ => null
               };
    }

    /// <summary>
    /// Group messages by appropriate time periods based on the filter
    /// </summary>
    /// <param name="query">Message query</param>
    /// <param name="filter">Selected time filter</param>
    /// <returns>Grouped data with labels and counts</returns>
    private List<(string Label, int Count)> GroupMessages(IQueryable<Data.Entity.Tables.Statistics.DiscordMessageEntity> query, TimeFilter filter)
    {
        return filter switch
               {
                   TimeFilter.Days30 => query.GroupBy(m => m.TimeStamp.Date)
                                             .Select(g => new { Date = g.Key, Count = g.Count() })
                                             .OrderBy(g => g.Date)
                                             .ToList()
                                             .Select(g => (g.Date.ToString("dd.MM", LocalizationGroup.CultureInfo), g.Count))
                                             .ToList(),

                   TimeFilter.Days90 => query.GroupBy(m => new { m.TimeStamp.Year, Week = ((m.TimeStamp.DayOfYear - 1) / 7) + 1 })
                                             .Select(g => new { g.Key.Year, g.Key.Week, Count = g.Count() })
                                             .OrderBy(g => g.Year)
                                             .ThenBy(g => g.Week)
                                             .ToList()
                                             .Select(g => ($"W{g.Week}/{g.Year}", g.Count))
                                             .ToList(),

                   TimeFilter.Days180 => query.GroupBy(m => new { m.TimeStamp.Year, Week = ((m.TimeStamp.DayOfYear - 1) / 7) + 1 })
                                              .Select(g => new { g.Key.Year, g.Key.Week, Count = g.Count() })
                                              .OrderBy(g => g.Year)
                                              .ThenBy(g => g.Week)
                                              .ToList()
                                              .Select(g => ($"W{g.Week}/{g.Year}", g.Count))
                                              .ToList(),

                   TimeFilter.Year1 => query.GroupBy(m => new { m.TimeStamp.Year, m.TimeStamp.Month })
                                            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                                            .OrderBy(g => g.Year)
                                            .ThenBy(g => g.Month)
                                            .ToList()
                                            .Select(g => ($"{g.Month:D2}/{g.Year}", g.Count))
                                            .ToList(),

                   _ => query.GroupBy(m => new { m.TimeStamp.Year, m.TimeStamp.Month })
                             .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                             .OrderBy(g => g.Year)
                             .ThenBy(g => g.Month)
                             .ToList()
                             .Select(g => ($"{g.Month:D2}/{g.Year}", g.Count))
                             .ToList()
               };
    }

    /// <summary>
    /// Calculate a simple linear trend line
    /// </summary>
    /// <param name="values">Data values</param>
    /// <returns>Trend line values</returns>
    private double[] CalculateTrend(double[] values)
    {
        if (values.Length == 0)
        {
            return [];
        }

        var n = values.Length;
        var sumX = 0.0;
        var sumY = 0.0;
        var sumXy = 0.0;
        var sumX2 = 0.0;

        for (var i = 0; i < n; i++)
        {
            sumX += i;
            sumY += values[i];
            sumXy += i * values[i];
            sumX2 += i * i;
        }

        var denominator = (n * sumX2) - (sumX * sumX);

        if (denominator == 0)
        {
            var avg = sumY / n;

            return Enumerable.Repeat(avg, n).ToArray();
        }

        var slope = ((n * sumXy) - (sumX * sumY)) / denominator;
        var intercept = (sumY - (slope * sumX)) / n;

        var trend = new double[n];

        for (var i = 0; i < n; i++)
        {
            trend[i] = Math.Max(0, intercept + (slope * i));
        }

        return trend;
    }

    /// <summary>
    /// Handle filter change
    /// </summary>
    /// <param name="filter">Selected filter</param>
    private void OnFilterChanged(TimeFilter filter)
    {
        if (_isLoading)
        {
            return;
        }

        _selectedFilter = filter;
        _isLoading = true;

        Task.Run(LoadDataAsync);
    }

    /// <summary>
    /// Load all chart data
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task LoadDataAsync()
    {
        _isLoading = true;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);

        await Task.Run(() =>
                       {
                           var cutoff = GetCutoffDate(_selectedFilter);

                           HashSet<ulong> ignoredChannelIds;

                           using (var repositoryFactory = RepositoryFactory.CreateInstance())
                           {
                               ignoredChannelIds = repositoryFactory.GetRepository<DiscordIgnoreChannelRepository>()
                                                                    .GetQuery()
                                                                    .Where(c => c.DiscordServerId == WebAppConfiguration.DiscordServerId)
                                                                    .Select(c => c.DiscordChannelId)
                                                                    .ToHashSet();
                           }

                           BuildOverviewChart(cutoff, ignoredChannelIds);
                           BuildTopUsersChart(cutoff, ignoredChannelIds);
                           BuildTopChannelsChart(cutoff, ignoredChannelIds);
                       })
                  .ConfigureAwait(false);

        _isLoading = false;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    /// <summary>
    /// Build the overview bar chart with trend line
    /// </summary>
    /// <param name="cutoff">Optional cutoff date</param>
    /// <param name="ignoredChannelIds">Channel IDs to exclude</param>
    private void BuildOverviewChart(DateTime? cutoff, HashSet<ulong> ignoredChannelIds)
    {
        using (var repositoryFactory = RepositoryFactory.CreateInstance())
        {
            var query = repositoryFactory.GetRepository<DiscordMessageRepository>()
                                         .GetQuery()
                                         .Where(m => m.DiscordServerId == WebAppConfiguration.DiscordServerId
                                                     && ignoredChannelIds.Contains(m.DiscordChannelId) == false);

            if (cutoff != null)
            {
                query = query.Where(m => m.TimeStamp >= cutoff.Value);
            }

            var grouped = GroupMessages(query, _selectedFilter);

            if (grouped.Count == 0)
            {
                _overviewChartData = null;

                return;
            }

            var labels = grouped.Select(g => g.Label).ToArray();
            var values = grouped.Select(g => (double)g.Count).ToArray();
            var trendValues = CalculateTrend(values);

            _overviewChartData = new ChartData
                                 {
                                     Labels = labels,
                                     Datasets =
                                     [
                                         new DataSet
                                         {
                                             Label = LocalizationGroup.GetText("MessagesLabel", "Messages"),
                                             Data = values,
                                             BackgroundColor = values.Select(_ => WebAppConfiguration.Colors.Accent)
                                                                     .ToArray()
                                         },
                                         new DataSet
                                         {
                                             Label = LocalizationGroup.GetText("TrendLabel", "Trend"),
                                             Type = "line",
                                             Data = trendValues,
                                             BorderColor = ["#ff6384"],
                                             BorderWidth = 2,
                                             PointRadius = 0,
                                             BackgroundColor = ["transparent"]
                                         }
                                     ]
                                 };
        }
    }

    /// <summary>
    /// Build the top users pie chart
    /// </summary>
    /// <param name="cutoff">Optional cutoff date</param>
    /// <param name="ignoredChannelIds">Channel IDs to exclude</param>
    private void BuildTopUsersChart(DateTime? cutoff, HashSet<ulong> ignoredChannelIds)
    {
        using (var repositoryFactory = RepositoryFactory.CreateInstance())
        {
            var query = repositoryFactory.GetRepository<DiscordMessageRepository>()
                                         .GetQuery()
                                         .Where(m => m.DiscordServerId == WebAppConfiguration.DiscordServerId
                                                     && ignoredChannelIds.Contains(m.DiscordChannelId) == false);

            if (cutoff != null)
            {
                query = query.Where(m => m.TimeStamp >= cutoff.Value);
            }

            var allUsers = query.GroupBy(m => m.DiscordAccountId)
                                .Select(g => new
                                             {
                                                 AccountId = g.Key,
                                                 Count = g.Count()
                                             })
                                .OrderByDescending(g => g.Count)
                                .ToList();

            if (allUsers.Count == 0)
            {
                _topUsersChartData = null;
                _allUsersTableData = null;

                return;
            }

            var nameMap = repositoryFactory.GetRepository<DiscordServerMemberRepository>()
                                           .GetQuery()
                                           .Where(m => m.ServerId == WebAppConfiguration.DiscordServerId)
                                           .Select(m => new { m.AccountId, m.Name })
                                           .ToDictionary(m => m.AccountId, m => m.Name);

            string ResolveName(ulong accountId) => nameMap.TryGetValue(accountId, out var name) ? name : accountId.ToString();

            var totalAll = allUsers.Sum(u => u.Count);
            var topEntries = allUsers.Take(TopCount)
                                     .Select(u => new
                                                  {
                                                      Name = ResolveName(u.AccountId),
                                                      u.Count
                                                  })
                                     .ToList();

            var topSum = topEntries.Sum(e => e.Count);
            var otherCount = totalAll - topSum;
            var labels = topEntries.Select(e => $"{e.Name} ({e.Count:N0})").ToList();
            var values = topEntries.Select(e => (double)e.Count).ToList();

            if (otherCount > 0)
            {
                labels.Add($"{LocalizationGroup.GetText("Other", "Other")} ({otherCount:N0})");
                values.Add(otherCount);
            }

            _topUsersChartData = new ChartData
                                 {
                                     Labels = labels.ToArray(),
                                     Datasets =
                                     [
                                         new DataSet
                                         {
                                             Data = values.ToArray(),
                                             BackgroundColor = _pieColors.Take(labels.Count).ToArray()
                                         }
                                     ]
                                 };

            _allUsersTableData = allUsers.Select(u => (Name: ResolveName(u.AccountId), u.Count))
                                         .ToList();
        }
    }

    /// <summary>
    /// Build the top channels pie chart
    /// </summary>
    /// <param name="cutoff">Optional cutoff date</param>
    /// <param name="ignoredChannelIds">Channel IDs to exclude</param>
    private void BuildTopChannelsChart(DateTime? cutoff, HashSet<ulong> ignoredChannelIds)
    {
        using (var repositoryFactory = RepositoryFactory.CreateInstance())
        {
            var query = repositoryFactory.GetRepository<DiscordMessageRepository>()
                                         .GetQuery()
                                         .Where(m => m.DiscordServerId == WebAppConfiguration.DiscordServerId
                                                     && ignoredChannelIds.Contains(m.DiscordChannelId) == false);

            if (cutoff != null)
            {
                query = query.Where(m => m.TimeStamp >= cutoff.Value);
            }

            var allChannels = query.GroupBy(m => m.DiscordChannelId)
                                   .Select(g => new
                                   {
                                       ChannelId = g.Key,
                                       Count = g.Count()
                                   })
                                   .OrderByDescending(g => g.Count)
                                   .ToList();

            if (allChannels.Count == 0)
            {
                _topChannelsChartData = null;
                _allChannelsTableData = null;

                return;
            }

            var channelNameMap = repositoryFactory.GetRepository<DiscordServerChannelRepository>()
                                                  .GetQuery()
                                                  .Where(c => c.ServerId == WebAppConfiguration.DiscordServerId)
                                                  .Select(c => new { c.ChannelId, c.Name })
                                                  .ToDictionary(c => c.ChannelId, c => c.Name);

            string ResolveChannelName(ulong channelId) => channelNameMap.TryGetValue(channelId, out var name) ? name : channelId.ToString();

            var totalAll = allChannels.Sum(c => c.Count);

            var topEntries = allChannels.Take(TopCount)
                                        .Select(c => new
                                                     {
                                                         Name = ResolveChannelName(c.ChannelId),
                                                         c.Count
                                                     })
                                        .ToList();

            var topSum = topEntries.Sum(e => e.Count);
            var otherCount = totalAll - topSum;

            var labels = topEntries.Select(e => $"{e.Name} ({e.Count:N0})").ToList();
            var values = topEntries.Select(e => (double)e.Count).ToList();

            if (otherCount > 0)
            {
                labels.Add($"{LocalizationGroup.GetText("Other", "Other")} ({otherCount:N0})");
                values.Add(otherCount);
            }

            _topChannelsChartData = new ChartData
                                    {
                                        Labels = labels.ToArray(),
                                        Datasets = [
                                                       new DataSet
                                                       {
                                                           Data = values.ToArray(),
                                                           BackgroundColor = _pieColors.Take(labels.Count).ToArray()
                                                       }
                                                   ]
                                    };

            _allChannelsTableData = allChannels.Select(c => (Name: ResolveChannelName(c.ChannelId), c.Count))
                                               .ToList();
        }
    }

    #endregion // Methods

    #region ComponentBase

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (_isLoading == false)
        {
            _isLoading = true;

            Task.Run(LoadDataAsync);
        }
    }

    #endregion // ComponentBase
}
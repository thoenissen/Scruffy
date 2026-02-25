using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Statistics;
using Scruffy.Data.Entity.Tables.Statistics;
using Scruffy.Data.Json.ChartJs;
using Scruffy.WebApp.Components.Base;
using Scruffy.WebApp.Components.Controls;

namespace Scruffy.WebApp.Components.Pages.Statistics;

/// <summary>
/// Discord voice statistics page
/// </summary>
[Authorize(Roles = "Member")]
public partial class DiscordVoiceStatisticsPage : LocatedComponent
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
    private List<(ulong AccountId, string Name, string AvatarUrl, double Hours)> _allUsersTableData;

    /// <summary>
    /// All channels table data
    /// </summary>
    private List<(ulong ChannelId, string Name, double Hours)> _allChannelsTableData;

    /// <summary>
    /// Whether the drilldown overlay is visible
    /// </summary>
    private bool _isDrilldownOpen;

    /// <summary>
    /// Whether the drilldown data is being loaded
    /// </summary>
    private bool _isDrilldownLoading;

    /// <summary>
    /// Title of the drilldown overlay
    /// </summary>
    private string _drilldownTitle;

    /// <summary>
    /// Avatar URL for a user drilldown (null for channel drilldown)
    /// </summary>
    private string _drilldownAvatarUrl;

    /// <summary>
    /// Chart data for the drilldown overlay
    /// </summary>
    private ChartData _drilldownChartData;

    /// <summary>
    /// Breakdown pie chart data (channels for user drilldown, users for channel drilldown)
    /// </summary>
    private ChartData _drilldownBreakdownChartData;

    /// <summary>
    /// Title for the breakdown pie chart
    /// </summary>
    private string _drilldownBreakdownTitle;

    /// <summary>
    /// Drilldown chart options (bar chart with trend)
    /// </summary>
    private ChartOptions _drilldownChartOptions;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public DiscordVoiceStatisticsPage()
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

        _drilldownChartOptions = new ChartOptions
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
    /// Format a duration in hours as a human-readable string
    /// </summary>
    /// <param name="totalHours">Total hours</param>
    /// <returns>Formatted duration string</returns>
    private string FormatDuration(double totalHours)
    {
        var hours = (int)totalHours;
        var minutes = (int)((totalHours - hours) * 60);

        return $"{hours.ToString("N0", LocalizationGroup.CultureInfo)}h {minutes:D2}m";
    }

    /// <summary>
    /// Group voice time spans by appropriate time periods based on the filter
    /// </summary>
    /// <param name="query">Voice time span query</param>
    /// <param name="filter">Selected time filter</param>
    /// <returns>Grouped data with labels and hours</returns>
    private List<(string Label, double Hours)> GroupVoiceTimeSpans(IQueryable<DiscordVoiceTimeSpanEntity> query, TimeFilter filter)
    {
        return filter switch
               {
                   TimeFilter.Days30 => query.GroupBy(v => v.StartTimeStamp.Date)
                                             .Select(g => new { Date = g.Key, TotalSeconds = g.Sum(v => (long)EF.Functions.DateDiffSecond(v.StartTimeStamp, v.EndTimeStamp)) })
                                             .OrderBy(g => g.Date)
                                             .ToList()
                                             .Select(g => (g.Date.ToString("dd.MM", LocalizationGroup.CultureInfo), g.TotalSeconds / 3600.0))
                                             .ToList(),

                   TimeFilter.Days90 => query.GroupBy(v => new { v.StartTimeStamp.Year, Week = ((v.StartTimeStamp.DayOfYear - 1) / 7) + 1 })
                                             .Select(g => new { g.Key.Year, g.Key.Week, TotalSeconds = g.Sum(v => (long)EF.Functions.DateDiffSecond(v.StartTimeStamp, v.EndTimeStamp)) })
                                             .OrderBy(g => g.Year)
                                             .ThenBy(g => g.Week)
                                             .ToList()
                                             .Select(g => ($"W{g.Week}/{g.Year}", g.TotalSeconds / 3600.0))
                                             .ToList(),

                   TimeFilter.Days180 => query.GroupBy(v => new { v.StartTimeStamp.Year, Week = ((v.StartTimeStamp.DayOfYear - 1) / 7) + 1 })
                                              .Select(g => new { g.Key.Year, g.Key.Week, TotalSeconds = g.Sum(v => (long)EF.Functions.DateDiffSecond(v.StartTimeStamp, v.EndTimeStamp)) })
                                              .OrderBy(g => g.Year)
                                              .ThenBy(g => g.Week)
                                              .ToList()
                                              .Select(g => ($"W{g.Week}/{g.Year}", g.TotalSeconds / 3600.0))
                                              .ToList(),

                   TimeFilter.Year1 => query.GroupBy(v => new { v.StartTimeStamp.Year, v.StartTimeStamp.Month })
                                            .Select(g => new { g.Key.Year, g.Key.Month, TotalSeconds = g.Sum(v => (long)EF.Functions.DateDiffSecond(v.StartTimeStamp, v.EndTimeStamp)) })
                                            .OrderBy(g => g.Year)
                                            .ThenBy(g => g.Month)
                                            .ToList()
                                            .Select(g => ($"{g.Month:D2}/{g.Year}", g.TotalSeconds / 3600.0))
                                            .ToList(),

                   _ => query.GroupBy(v => new { v.StartTimeStamp.Year, v.StartTimeStamp.Month })
                             .Select(g => new { g.Key.Year, g.Key.Month, TotalSeconds = g.Sum(v => (long)EF.Functions.DateDiffSecond(v.StartTimeStamp, v.EndTimeStamp)) })
                             .OrderBy(g => g.Year)
                             .ThenBy(g => g.Month)
                             .ToList()
                             .Select(g => ($"{g.Month:D2}/{g.Year}", g.TotalSeconds / 3600.0))
                             .ToList()
               };
    }

    /// <summary>
    /// Fill gaps in a timeline so that every expected time slot is present (with 0 hours if no data exists)
    /// </summary>
    /// <param name="grouped">Grouped data from <see cref="GroupVoiceTimeSpans"/></param>
    /// <param name="filter">Selected time filter</param>
    /// <param name="startDate">Start date of the timeline range</param>
    /// <returns>Complete timeline with zero-filled gaps</returns>
    private List<(string Label, double Hours)> FillTimelineGaps(List<(string Label, double Hours)> grouped, TimeFilter filter, DateTime startDate)
    {
        if (grouped.Count == 0)
        {
            return grouped;
        }

        var today = DateTime.UtcNow.Date;
        var lookup = grouped.ToDictionary(g => g.Label, g => g.Hours);
        var result = new List<(string Label, double Hours)>();

        switch (filter)
        {
            case TimeFilter.Days30:
                for (var date = startDate.Date; date <= today; date = date.AddDays(1))
                {
                    var label = date.ToString("dd.MM", LocalizationGroup.CultureInfo);
                    result.Add((label, lookup.GetValueOrDefault(label)));
                }

                break;

            case TimeFilter.Days90:
            case TimeFilter.Days180:
                var seenWeeks = new HashSet<string>();

                for (var date = startDate.Date; date <= today; date = date.AddDays(1))
                {
                    var week = ((date.DayOfYear - 1) / 7) + 1;
                    var label = $"W{week}/{date.Year}";

                    if (seenWeeks.Add(label))
                    {
                        result.Add((label, lookup.GetValueOrDefault(label)));
                    }
                }

                break;

            case TimeFilter.Year1:
            default:
                var currentMonth = new DateTime(startDate.Year, startDate.Month, 1);
                var endMonth = new DateTime(today.Year, today.Month, 1);

                while (currentMonth <= endMonth)
                {
                    var label = $"{currentMonth.Month:D2}/{currentMonth.Year}";
                    result.Add((label, lookup.GetValueOrDefault(label)));
                    currentMonth = currentMonth.AddMonths(1);
                }

                break;
        }

        return result;
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

                           HashSet<ulong> memberAccountIds;

                           using (var repositoryFactory = RepositoryFactory.CreateInstance())
                           {
                               memberAccountIds = repositoryFactory.GetRepository<DiscordServerMemberRepository>()
                                                                   .GetQuery()
                                                                   .Where(m => m.ServerId == WebAppConfiguration.DiscordServerId
                                                                               && m.IsBot == false)
                                                                   .Select(m => m.AccountId)
                                                                   .ToHashSet();
                           }

                           BuildOverviewChart(cutoff, memberAccountIds);
                           BuildTopUsersChart(cutoff, memberAccountIds);
                           BuildTopChannelsChart(cutoff, memberAccountIds);
                       })
                  .ConfigureAwait(false);

        _isLoading = false;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    /// <summary>
    /// Build the overview bar chart with trend line
    /// </summary>
    /// <param name="cutoff">Optional cutoff date</param>
    /// <param name="memberAccountIds">Account IDs of current non-bot server members</param>
    private void BuildOverviewChart(DateTime? cutoff, HashSet<ulong> memberAccountIds)
    {
        using (var repositoryFactory = RepositoryFactory.CreateInstance())
        {
            var query = repositoryFactory.GetRepository<DiscordVoiceTimeSpanRepository>()
                                         .GetQuery()
                                         .Where(v => v.DiscordServerId == WebAppConfiguration.DiscordServerId
                                                     && v.IsCompleted
                                                     && memberAccountIds.Contains(v.DiscordAccountId));

            if (cutoff != null)
            {
                query = query.Where(v => v.StartTimeStamp >= cutoff.Value);
            }

            var grouped = GroupVoiceTimeSpans(query, _selectedFilter);

            if (grouped.Count == 0)
            {
                _overviewChartData = null;

                return;
            }

            var startDate = cutoff ?? query.Min(v => v.StartTimeStamp);
            grouped = FillTimelineGaps(grouped, _selectedFilter, startDate);

            var labels = grouped.Select(g => g.Label).ToArray();
            var values = grouped.Select(g => g.Hours).ToArray();
            var trendValues = CalculateTrend(values);

            _overviewChartData = new ChartData
                                 {
                                     Labels = labels,
                                     Datasets =
                                     [
                                         new DataSet
                                         {
                                             Label = LocalizationGroup.GetText("HoursLabel", "Hours"),
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
    /// <param name="memberAccountIds">Account IDs of current non-bot server members</param>
    private void BuildTopUsersChart(DateTime? cutoff, HashSet<ulong> memberAccountIds)
    {
        using (var repositoryFactory = RepositoryFactory.CreateInstance())
        {
            var query = repositoryFactory.GetRepository<DiscordVoiceTimeSpanRepository>()
                                         .GetQuery()
                                         .Where(v => v.DiscordServerId == WebAppConfiguration.DiscordServerId
                                                     && v.IsCompleted
                                                     && memberAccountIds.Contains(v.DiscordAccountId));

            if (cutoff != null)
            {
                query = query.Where(v => v.StartTimeStamp >= cutoff.Value);
            }

            var allUsers = query.GroupBy(v => v.DiscordAccountId)
                                .Select(g => new
                                             {
                                                 AccountId = g.Key,
                                                 TotalSeconds = g.Sum(v => (long)EF.Functions.DateDiffSecond(v.StartTimeStamp, v.EndTimeStamp))
                                             })
                                .OrderByDescending(g => g.TotalSeconds)
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
                                           .Select(m => new { m.AccountId, m.Name, m.AvatarUrl })
                                           .ToDictionary(m => m.AccountId, m => new { m.Name, m.AvatarUrl });

            string ResolveName(ulong accountId) => nameMap.TryGetValue(accountId, out var member) ? member.Name : accountId.ToString();

            var allUsersWithHours = allUsers.Select(u => new { u.AccountId, Hours = u.TotalSeconds / 3600.0 }).ToList();
            var totalAll = allUsersWithHours.Sum(u => u.Hours);
            var topEntries = allUsersWithHours.Take(TopCount)
                                              .Select(u => new
                                                           {
                                                               Name = ResolveName(u.AccountId),
                                                               u.Hours
                                                           })
                                              .ToList();

            var topSum = topEntries.Sum(e => e.Hours);
            var otherHours = totalAll - topSum;
            var labels = topEntries.Select(e => $"{e.Name} ({FormatDuration(e.Hours)})").ToList();
            var values = topEntries.Select(e => e.Hours).ToList();

            if (otherHours > 0)
            {
                labels.Add($"{LocalizationGroup.GetText("Other", "Other")} ({FormatDuration(otherHours)})");
                values.Add(otherHours);
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

            _allUsersTableData = allUsersWithHours.Select(u => (u.AccountId,
                                                                Name: ResolveName(u.AccountId),
                                                                AvatarUrl: nameMap.TryGetValue(u.AccountId, out var member) ? member.AvatarUrl : null,
                                                                u.Hours))
                                                  .ToList();
        }
    }

    /// <summary>
    /// Build the top channels pie chart
    /// </summary>
    /// <param name="cutoff">Optional cutoff date</param>
    /// <param name="memberAccountIds">Account IDs of current non-bot server members</param>
    private void BuildTopChannelsChart(DateTime? cutoff, HashSet<ulong> memberAccountIds)
    {
        using (var repositoryFactory = RepositoryFactory.CreateInstance())
        {
            var query = repositoryFactory.GetRepository<DiscordVoiceTimeSpanRepository>()
                                         .GetQuery()
                                         .Where(v => v.DiscordServerId == WebAppConfiguration.DiscordServerId
                                                     && v.IsCompleted
                                                     && memberAccountIds.Contains(v.DiscordAccountId));

            if (cutoff != null)
            {
                query = query.Where(v => v.StartTimeStamp >= cutoff.Value);
            }

            var allChannels = query.GroupBy(v => v.DiscordChannelId)
                                   .Select(g => new
                                   {
                                       ChannelId = g.Key,
                                       TotalSeconds = g.Sum(v => (long)EF.Functions.DateDiffSecond(v.StartTimeStamp, v.EndTimeStamp))
                                   })
                                   .OrderByDescending(g => g.TotalSeconds)
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

            var allChannelsWithHours = allChannels.Select(c => new { c.ChannelId, Hours = c.TotalSeconds / 3600.0 }).ToList();
            var totalAll = allChannelsWithHours.Sum(c => c.Hours);

            var topEntries = allChannelsWithHours.Take(TopCount)
                                                 .Select(c => new
                                                              {
                                                                  Name = ResolveChannelName(c.ChannelId),
                                                                  c.Hours
                                                              })
                                                 .ToList();

            var topSum = topEntries.Sum(e => e.Hours);
            var otherHours = totalAll - topSum;

            var labels = topEntries.Select(e => $"{e.Name} ({FormatDuration(e.Hours)})").ToList();
            var values = topEntries.Select(e => e.Hours).ToList();

            if (otherHours > 0)
            {
                labels.Add($"{LocalizationGroup.GetText("Other", "Other")} ({FormatDuration(otherHours)})");
                values.Add(otherHours);
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

            _allChannelsTableData = allChannelsWithHours.Select(c => (c.ChannelId, Name: ResolveChannelName(c.ChannelId), c.Hours))
                                                        .ToList();
        }
    }

    /// <summary>
    /// Open the drilldown overlay for a specific user
    /// </summary>
    /// <param name="accountId">Discord account ID</param>
    /// <param name="userName">Display name of the user</param>
    /// <param name="avatarUrl">Avatar URL of the user</param>
    private void OnUserClicked(ulong accountId, string userName, string avatarUrl)
    {
        _drilldownTitle = userName;
        _drilldownAvatarUrl = avatarUrl;
        _drilldownChartData = null;
        _isDrilldownOpen = true;
        _isDrilldownLoading = true;

        Task.Run(() => LoadDrilldownAsync(accountId: accountId));
    }

    /// <summary>
    /// Open the drilldown overlay for a specific channel
    /// </summary>
    /// <param name="channelId">Discord channel ID</param>
    /// <param name="channelName">Display name of the channel</param>
    private void OnChannelClicked(ulong channelId, string channelName)
    {
        _drilldownTitle = $"#{channelName}";
        _drilldownAvatarUrl = null;
        _drilldownChartData = null;
        _isDrilldownOpen = true;
        _isDrilldownLoading = true;

        Task.Run(() => LoadDrilldownAsync(channelId: channelId));
    }

    /// <summary>
    /// Load drilldown chart data for a specific user or channel
    /// </summary>
    /// <param name="accountId">Discord account ID (for user drilldown)</param>
    /// <param name="channelId">Discord channel ID (for channel drilldown)</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task LoadDrilldownAsync(ulong? accountId = null, ulong? channelId = null)
    {
        await Task.Run(() =>
                       {
                           var cutoff = GetCutoffDate(_selectedFilter);

                           using (var repositoryFactory = RepositoryFactory.CreateInstance())
                           {
                               var memberAccountIds = repositoryFactory.GetRepository<DiscordServerMemberRepository>()
                                                                       .GetQuery()
                                                                       .Where(m => m.ServerId == WebAppConfiguration.DiscordServerId
                                                                                   && m.IsBot == false)
                                                                       .Select(m => m.AccountId)
                                                                       .ToHashSet();

                               var query = repositoryFactory.GetRepository<DiscordVoiceTimeSpanRepository>()
                                                            .GetQuery()
                                                            .Where(v => v.DiscordServerId == WebAppConfiguration.DiscordServerId
                                                                        && v.IsCompleted
                                                                        && memberAccountIds.Contains(v.DiscordAccountId));

                               if (cutoff != null)
                               {
                                   query = query.Where(v => v.StartTimeStamp >= cutoff.Value);
                               }

                               if (accountId != null)
                               {
                                   query = query.Where(v => v.DiscordAccountId == accountId.Value);
                               }

                               if (channelId != null)
                               {
                                   query = query.Where(v => v.DiscordChannelId == channelId.Value);
                               }

                               BuildDrilldownTimelineChart(query, cutoff);
                               BuildDrilldownBreakdownChart(repositoryFactory, query, accountId != null);
                           }
                       })
                  .ConfigureAwait(false);

        _isDrilldownLoading = false;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    /// <summary>
    /// Build the timeline bar chart for the drilldown overlay
    /// </summary>
    /// <param name="query">Filtered voice time span query</param>
    /// <param name="cutoff">Optional cutoff date for gap filling</param>
    private void BuildDrilldownTimelineChart(IQueryable<DiscordVoiceTimeSpanEntity> query, DateTime? cutoff)
    {
        var grouped = GroupVoiceTimeSpans(query, _selectedFilter);

        if (grouped.Count == 0)
        {
            _drilldownChartData = null;

            return;
        }

        var startDate = cutoff ?? query.Min(v => v.StartTimeStamp);
        grouped = FillTimelineGaps(grouped, _selectedFilter, startDate);

        var labels = grouped.Select(g => g.Label).ToArray();
        var values = grouped.Select(g => g.Hours).ToArray();
        var trendValues = CalculateTrend(values);

        _drilldownChartData = new ChartData
                              {
                                  Labels = labels,
                                  Datasets =
                                  [
                                      new DataSet
                                      {
                                          Label = LocalizationGroup.GetText("HoursLabel", "Hours"),
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

    /// <summary>
    /// Build the breakdown pie chart for the drilldown overlay
    /// </summary>
    /// <param name="repositoryFactory">Repository factory</param>
    /// <param name="query">Filtered voice time span query</param>
    /// <param name="isUserDrilldown">True to show channels breakdown, false to show users breakdown</param>
    private void BuildDrilldownBreakdownChart(RepositoryFactory repositoryFactory, IQueryable<DiscordVoiceTimeSpanEntity> query, bool isUserDrilldown)
    {
        if (isUserDrilldown)
        {
            _drilldownBreakdownTitle = LocalizationGroup.GetText("DrilldownChannelsTitle", "Channels");

            var channelGroups = query.GroupBy(v => v.DiscordChannelId)
                                     .Select(g => new { ChannelId = g.Key, TotalSeconds = g.Sum(v => (long)EF.Functions.DateDiffSecond(v.StartTimeStamp, v.EndTimeStamp)) })
                                     .OrderByDescending(g => g.TotalSeconds)
                                     .ToList();

            if (channelGroups.Count == 0)
            {
                _drilldownBreakdownChartData = null;

                return;
            }

            var channelNameMap = repositoryFactory.GetRepository<DiscordServerChannelRepository>()
                                                  .GetQuery()
                                                  .Where(c => c.ServerId == WebAppConfiguration.DiscordServerId)
                                                  .Select(c => new { c.ChannelId, c.Name })
                                                  .ToDictionary(c => c.ChannelId, c => c.Name);

            string ResolveChannelName(ulong id) => channelNameMap.TryGetValue(id, out var name) ? name : id.ToString();

            var channelGroupsWithHours = channelGroups.Select(c => new { c.ChannelId, Hours = c.TotalSeconds / 3600.0 }).ToList();
            var totalAll = channelGroupsWithHours.Sum(c => c.Hours);
            var topEntries = channelGroupsWithHours.Take(TopCount)
                                                   .Select(c => new { Name = ResolveChannelName(c.ChannelId), c.Hours })
                                                   .ToList();

            var topSum = topEntries.Sum(e => e.Hours);
            var otherHours = totalAll - topSum;
            var labels = topEntries.Select(e => $"#{e.Name} ({FormatDuration(e.Hours)})").ToList();
            var values = topEntries.Select(e => e.Hours).ToList();

            if (otherHours > 0)
            {
                labels.Add($"{LocalizationGroup.GetText("Other", "Other")} ({FormatDuration(otherHours)})");
                values.Add(otherHours);
            }

            _drilldownBreakdownChartData = new ChartData
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
        }
        else
        {
            _drilldownBreakdownTitle = LocalizationGroup.GetText("DrilldownUsersTitle", "Users");

            var userGroups = query.GroupBy(v => v.DiscordAccountId)
                                  .Select(g => new { AccountId = g.Key, TotalSeconds = g.Sum(v => (long)EF.Functions.DateDiffSecond(v.StartTimeStamp, v.EndTimeStamp)) })
                                  .OrderByDescending(g => g.TotalSeconds)
                                  .ToList();

            if (userGroups.Count == 0)
            {
                _drilldownBreakdownChartData = null;

                return;
            }

            var nameMap = repositoryFactory.GetRepository<DiscordServerMemberRepository>()
                                           .GetQuery()
                                           .Where(m => m.ServerId == WebAppConfiguration.DiscordServerId)
                                           .Select(m => new { m.AccountId, m.Name })
                                           .ToDictionary(m => m.AccountId, m => m.Name);

            string ResolveName(ulong id) => nameMap.TryGetValue(id, out var name) ? name : id.ToString();

            var userGroupsWithHours = userGroups.Select(u => new { u.AccountId, Hours = u.TotalSeconds / 3600.0 }).ToList();
            var totalAll = userGroupsWithHours.Sum(u => u.Hours);
            var topEntries = userGroupsWithHours.Take(TopCount)
                                                .Select(u => new { Name = ResolveName(u.AccountId), u.Hours })
                                                .ToList();

            var topSum = topEntries.Sum(e => e.Hours);
            var otherHours = totalAll - topSum;
            var labels = topEntries.Select(e => $"{e.Name} ({FormatDuration(e.Hours)})").ToList();
            var values = topEntries.Select(e => e.Hours).ToList();

            if (otherHours > 0)
            {
                labels.Add($"{LocalizationGroup.GetText("Other", "Other")} ({FormatDuration(otherHours)})");
                values.Add(otherHours);
            }

            _drilldownBreakdownChartData = new ChartData
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
        }
    }

    /// <summary>
    /// Close the drilldown overlay
    /// </summary>
    private void OnCloseDrilldown()
    {
        _isDrilldownOpen = false;
        _drilldownChartData = null;
        _drilldownBreakdownChartData = null;
        _drilldownBreakdownTitle = null;
        _drilldownTitle = null;
        _drilldownAvatarUrl = null;
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
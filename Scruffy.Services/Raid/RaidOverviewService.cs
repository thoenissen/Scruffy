using Discord;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Json.QuickChart;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Extensions;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord.Interfaces;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Raid;

/// <summary>
/// Raid overview
/// </summary>
public class RaidOverviewService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// QuickChart-Connector
    /// </summary>
    private readonly QuickChartConnector _quickChartConnector;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="quickChartConnector">QuickChart-Connector</param>
    public RaidOverviewService(LocalizationService localizationService, QuickChartConnector quickChartConnector)
        : base(localizationService)
    {
        _quickChartConnector = quickChartConnector;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Participation points overview
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PostParticipationOverview(IContextContainer commandContext)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var users = dbFactory.GetRepository<RaidCurrentUserPointsRepository>()
                                 .GetQuery()
                                 .OrderByDescending(obj => obj.Points)
                                 .Select(obj => new
                                                {
                                                    UserId = obj.User
                                                                .DiscordAccounts
                                                                .Select(obj2 => obj2.Id)
                                                                .FirstOrDefault(),
                                                    Points = obj.Points * 100.0
                                                })
                                 .ToList();

            if (users.Count > 0)
            {
                var embedBuilder = new EmbedBuilder();
                embedBuilder.WithTitle(LocalizationGroup.GetText("ParticipationOverview", "Participation points overview"));
                embedBuilder.WithColor(Color.DarkBlue);
                embedBuilder.WithImageUrl("attachment://chart.png");

                var userNames = new List<string>();
                foreach (var user in users)
                {
                    var member = await commandContext.Guild
                                                     .GetUserAsync(user.UserId)
                                                     .ConfigureAwait(false);

                    userNames.Add($"{member.TryGetDisplayName()} [{user.Points:0.00}]");
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
                                                                           BackgroundColor = users.Select(obj => "#316ed5")
                                                                                                  .ToList(),
                                                                           BorderColor = "#274d85",
                                                                           Data = users.Select(obj => obj.Points)
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
                                                                                    new()
                                                                                    {
                                                                                        Ticks = new AxisTicks<double>
                                                                                                {
                                                                                                    MinValue = 0,
                                                                                                    MaxValue = Math.Ceiling(((users.Max(obj => obj.Points) / 10) + 1) * 10),
                                                                                                    FontColor = "#b3b3b3"
                                                                                                }
                                                                                    }
                                                                                },
                                                                        YAxes = new List<YAxis>
                                                                                {
                                                                                    new()
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
                    await commandContext.Channel
                                        .SendFileAsync(new FileAttachment(chartStream, "chart.png"), embed: embedBuilder.Build())
                                        .ConfigureAwait(false);
                }
            }
        }
    }

    #endregion // Methods
}
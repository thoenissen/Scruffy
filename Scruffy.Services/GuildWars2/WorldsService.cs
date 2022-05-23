using Discord;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.GameData;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Json.QuickChart;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.GuildWars2;

/// <summary>
/// World
/// </summary>
public class WorldsService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Fields
    /// </summary>
    private readonly QuickChartConnector _quickChartConnector;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="quickChartConnector">QuickChart-Connector</param>
    public WorldsService(LocalizationService localizationService, QuickChartConnector quickChartConnector)
        : base(localizationService)
    {
        _quickChartConnector = quickChartConnector;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Import worlds
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<bool> ImportWorlds()
    {
        var success = false;

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            try
            {
                var connector = new GuildWars2ApiConnector(null);
                await using (connector.ConfigureAwait(false))
                {
                    var worlds = await connector.GetWorlds().ConfigureAwait(false);

                    success = true;

                    foreach (var world in worlds)
                    {
                        if (dbFactory.GetRepository<GuildWarsWorldRepository>()
                                     .AddOrRefresh(obj => obj.Id == world.Id,
                                                   obj =>
                                                   {
                                                       obj.Id = world.Id;
                                                       obj.Name = world.Name;
                                                   }) == false)
                        {
                            success = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(WorldsService), ex.Message, null, ex);
            }
        }

        return success;
    }

    /// <summary>
    /// Post worlds overview
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PostWorldsOverview(InteractionContextContainer commandContext)
    {
        await commandContext.DeferProcessing()
                            .ConfigureAwait(false);

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var worldsQuery = dbFactory.GetRepository<GuildWarsWorldRepository>()
                                       .GetQuery()
                                       .Select(obj => obj);

            var worlds = dbFactory.GetRepository<AccountRepository>()
                                  .GetQuery()
                                  .Where(obj => obj.WorldId != null)
                                  .GroupBy(obj => obj.WorldId)
                                  .Select(obj => new
                                                 {
                                                     WorldId = obj.Key,
                                                     Count = obj.Count(),
                                                     Name = worldsQuery.Where(obj2 => obj2.Id == obj.Key)
                                                                       .Select(obj2 => obj2.Name)
                                                                       .FirstOrDefault()
                                                 })
                                  .ToList();

            if (worlds.Count > 0)
            {
                var embedBuilder = new EmbedBuilder();

                embedBuilder.WithTitle(LocalizationGroup.GetText("Overview", "Worlds overview"));
                embedBuilder.WithColor(Color.DarkBlue);
                embedBuilder.WithImageUrl("attachment://chart.png");

                var chartConfiguration = new ChartConfigurationData
                                         {
                                             Type = "bar",
                                             Data = new Data.Json.QuickChart.Data
                                                    {
                                                        DataSets = new List<DataSet>
                                                                   {
                                                                       new DataSet<int>
                                                                       {
                                                                           BackgroundColor = worlds.Select(obj => "#316ed5")
                                                                                                   .ToList(),
                                                                           BorderColor = "#274d85",
                                                                           Data = worlds.OrderByDescending(obj => obj.Count)
                                                                                        .ThenBy(obj => obj.Name)
                                                                                        .Select(obj => obj.Count)
                                                                                        .ToList()
                                                                       }
                                                                   },
                                                        Labels = worlds.OrderByDescending(obj => obj.Count)
                                                                       .ThenBy(obj => obj.Name)
                                                                       .Select(obj => obj.Name)
                                                                       .ToList()
                                                    },
                                             Options = new OptionsCollection
                                                       {
                                                           Scales = new ScalesCollection
                                                                    {
                                                                        XAxes = new List<XAxis>
                                                                                {
                                                                                    new()
                                                                                    {
                                                                                        Ticks = new AxisTicks
                                                                                                {
                                                                                                    FontColor = "#b3b3b3"
                                                                                                }
                                                                                    }
                                                                                },
                                                                        YAxes = new List<YAxis>
                                                                                {
                                                                                    new()
                                                                                    {
                                                                                        Ticks = new AxisTicks<int>
                                                                                                {
                                                                                                    MinValue = 0,
                                                                                                    MaxValue = ((worlds.Max(obj => obj.Count) / 10) + 1) * 10,
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
                                                                                  Height = 300,
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
                    await commandContext.ReplyAsync(embed: embedBuilder.Build(), attachments: new[] { new FileAttachment(chartStream, "chart.png") })
                                        .ConfigureAwait(false);
                }
            }
        }
    }

    #endregion // Methods
}
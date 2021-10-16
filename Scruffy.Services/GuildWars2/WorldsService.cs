using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Json.QuickChart;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.GuildWars2
{
    /// <summary>
    /// World
    /// </summary>
    public class WorldsService : LocatedServiceBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public WorldsService(LocalizationService localizationService)
            : base(localizationService)
        {
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
                    await using (var connector = new GuidWars2ApiConnector(null))
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
                    LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(WorldsService), ex.Message, ex.ToString());
                }
            }

            return success;
        }

        /// <summary>
        /// Post worlds overview
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PostWorldsOverview(CommandContextContainer commandContext)
        {
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
                    var embedBuilder = new DiscordEmbedBuilder();
                    var messageBuilder = new DiscordMessageBuilder();

                    embedBuilder.WithTitle(LocalizationGroup.GetText("Overview", "Worlds overview"));
                    embedBuilder.WithColor(DiscordColor.DarkBlue);
                    embedBuilder.WithImageUrl("attachment://chart.png");

                    await using (var connector = new QuickChartConnector())
                    {
                        var chartConfiguration = new ChartConfigurationData
                                                 {
                                                     Type = "bar",
                                                     Data = new Data.Json.QuickChart.Data
                                                     {
                                                         DataSets = new List<DataSet>
                                                                    {
                                                                        new DataSet<int>
                                                                        {
                                                                            BackgroundColor =  worlds.Select(obj => "#316ed5")
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
                                                                         new ()
                                                                         {
                                                                             Ticks = new AxisTicks
                                                                                     {
                                                                                         FontColor = "#b3b3b3"
                                                                                     }
                                                                         }
                                                                     },
                                                             YAxes = new List<YAxis>
                                                                     {
                                                                         new ()
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

                        await using (var chartStream = await connector.GetChartAsStream(new ChartData
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
                                                                      .ConfigureAwait(false))
                        {
                            messageBuilder.WithFile("chart.png", chartStream);
                            messageBuilder.WithEmbed(embedBuilder);

                            await commandContext.Channel
                                                .SendMessageAsync(messageBuilder)
                                                .ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        #endregion // Methods
    }
}

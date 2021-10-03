using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Json.QuickChart;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Raid
{
    /// <summary>
    /// Raid overview
    /// </summary>
    public class RaidOverviewService : LocatedServiceBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public RaidOverviewService(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Participation points overview
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PostParticipationOverview(CommandContextContainer commandContext)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var users = dbFactory.GetRepository<RaidCurrentUserPointsRepository>()
                                     .GetQuery()
                                     .OrderByDescending(obj => obj.Points)
                                     .Select(obj => new
                                                    {
                                                        obj.UserId,
                                                        Points = obj.Points * 100.0
                                                    })
                                     .ToList();

                if (users.Count > 0)
                {
                    var embedBuilder = new DiscordEmbedBuilder();
                    var messageBuilder = new DiscordMessageBuilder();
                    embedBuilder.WithTitle(LocalizationGroup.GetText("ParticipationOverview", "Participation points overview"));
                    embedBuilder.WithColor(DiscordColor.DarkBlue);
                    embedBuilder.WithImageUrl("attachment://chart.png");

                    var userNames = new List<string>();
                    foreach (var user in users)
                    {
                        var member = await commandContext.Guild
                                                         .GetMemberAsync(user.UserId)
                                                         .ConfigureAwait(false);

                        userNames.Add($"{(string.IsNullOrWhiteSpace(member.Nickname) ? string.IsNullOrWhiteSpace(member.DisplayName) ? member.Username : member.DisplayName : member.Nickname)} [{user.Points:0.00}]");
                    }

                    await using (var connector = new QuickChartConnector())
                    {
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
                                                                                            new XAxis
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
                                                                                            new YAxis
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

                        await using (var chartStream = await connector.GetChartAsStream(new ChartData
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
                                                                      .ConfigureAwait(false))
                        {
                            messageBuilder.WithFile("chart.png", chartStream);
                            messageBuilder.WithEmbed(embedBuilder);

                            await commandContext.Channel.SendMessageAsync(messageBuilder)
                                                .ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        #endregion // Methods
    }
}
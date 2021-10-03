using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildAdministration;
using Scruffy.Data.Json.QuickChart;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.GuildAdministration
{
    /// <summary>
    /// Special rank service
    /// </summary>
    public class GuildSpecialRankService : LocatedServiceBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public GuildSpecialRankService(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Post a overview of the current points
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PostOverview(CommandContextContainer commandContext)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                foreach (var configuration in dbFactory.GetRepository<GuildSpecialRankConfigurationRepository>()
                                                       .GetQuery()
                                                       .Where(obj => obj.IsDeleted == false && obj.Guild.DiscordServerId == commandContext.Guild.Id)
                                                       .Select(obj => new
                                                                      {
                                                                          obj.Description,
                                                                          obj.DiscordRoleId,
                                                                          obj.GrantThreshold,
                                                                          obj.RemoveThreshold,
                                                                          Users = obj.GuildSpecialRankPoints.Where(obj2 => obj2.Points > 0)
                                                                                     .Select(obj2 => new
                                                                                                     {
                                                                                                         obj2.UserId,
                                                                                                         obj2.Points
                                                                                                     })
                                                                                     .ToList()
                                                                      })
                                                       .ToList())
                {
                    if (configuration.Users.Count > 0)
                    {
                        var embedBuilder = new DiscordEmbedBuilder();
                        var messageBuilder = new DiscordMessageBuilder();

                        embedBuilder.WithTitle(LocalizationGroup.GetText("Overview", "Points overview"));
                        embedBuilder.WithDescription($"{configuration.Description} ({commandContext.Guild.GetRole(configuration.DiscordRoleId).Mention})");
                        embedBuilder.WithColor(DiscordColor.DarkBlue);
                        embedBuilder.WithImageUrl("attachment://chart.png");

                        await using (var connector = new QuickChartConnector())
                        {
                            var users = new List<string>();
                            foreach (var user in configuration.Users
                                                              .OrderByDescending(obj => obj.Points)
                                                              .ThenBy(obj => obj.UserId))
                            {
                                var member = await commandContext.Guild
                                                                 .GetMemberAsync(user.UserId)
                                                                 .ConfigureAwait(false);

                                users.Add($"{(string.IsNullOrWhiteSpace(member.Nickname) ? string.IsNullOrWhiteSpace(member.DisplayName) ? member.Username : member.DisplayName : member.Nickname)} ({user.Points:0.##})");
                            }

                            var chartConfiguration = new ChartConfigurationData
                                                     {
                                                         Type = "bar",
                                                         Data = new Data.Json.QuickChart.Data
                                                                {
                                                                    DataSets = new List<DataSet>
                                                                               {
                                                                                   new DataSet<double>
                                                                                   {
                                                                                       BackgroundColor = configuration.Users
                                                                                                                      .Select(obj => "#316ed5")
                                                                                                                      .ToList(),
                                                                                       BorderColor = "#274d85",
                                                                                       Data = configuration.Users
                                                                                                           .OrderByDescending(obj => obj.Points)
                                                                                                           .ThenBy(obj => obj.UserId)
                                                                                                           .Select(obj => obj.Points)
                                                                                                           .ToList()
                                                                                   }
                                                                               },
                                                                    Labels = users
                                                                },
                                                         Options = new OptionsCollection
                                                                   {
                                                                       Annotation = new AnnotationsCollection
                                                                                    {
                                                                                        Annotations = new List<Annotation>
                                                                                                      {
                                                                                                          new Annotation
                                                                                                          {
                                                                                                              BorderColor = "#f45b5b",
                                                                                                              BorderWidth = 2,
                                                                                                              Mode = "horizontal",
                                                                                                              ScaleID = "y-axis-0",
                                                                                                              Type = "line",
                                                                                                              Value = configuration.RemoveThreshold
                                                                                                          },
                                                                                                          new Annotation
                                                                                                          {
                                                                                                              BorderColor = "#90ee7e",
                                                                                                              BorderWidth = 2,
                                                                                                              Mode = "horizontal",
                                                                                                              ScaleID = "y-axis-0",
                                                                                                              Type = "line",
                                                                                                              Value = configuration.GrantThreshold
                                                                                                          }
                                                                                                      }
                                                                                    },
                                                                       Scales = new ScalesCollection
                                                                                {
                                                                                    XAxes = new List<XAxis>
                                                                                            {
                                                                                                new XAxis
                                                                                                {
                                                                                                    Ticks = new AxisTicks
                                                                                                            {
                                                                                                                FontColor = "#b3b3b3"
                                                                                                            }
                                                                                                }
                                                                                            },
                                                                                    YAxes = new List<YAxis>
                                                                                            {
                                                                                                new YAxis
                                                                                                {
                                                                                                    Ticks = new AxisTicks
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
        }

        #endregion // Methods
    }
}
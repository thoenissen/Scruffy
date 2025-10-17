using Discord;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Json.QuickChart;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Extensions;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Guild;

/// <summary>
/// Special rank service
/// </summary>
public class GuildSpecialRankService : LocatedServiceBase
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
    public GuildSpecialRankService(LocalizationService localizationService, QuickChartConnector quickChartConnector)
        : base(localizationService)
    {
        _quickChartConnector = quickChartConnector;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Post a overview of the current points
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PostOverview(InteractionContextContainer commandContext)
    {
        await commandContext.DeferProcessing()
                            .ConfigureAwait(false);

        var isFirstReply = true;

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
                                                                                                     UserId = obj2.User
                                                                                                                  .DiscordAccounts
                                                                                                                  .Select(obj3 => obj3.Id)
                                                                                                                  .FirstOrDefault(),
                                                                                                     obj2.Points
                                                                                                 })
                                                                                 .ToList()
                                                                  })
                                                   .ToList())
            {
                if (configuration.Users.Count > 0)
                {
                    var embedBuilder = new EmbedBuilder();

                    embedBuilder.WithTitle(LocalizationGroup.GetText("Overview", "Points overview"));
                    embedBuilder.WithDescription($"{configuration.Description} ({commandContext.Guild.GetRole(configuration.DiscordRoleId).Mention})");
                    embedBuilder.WithColor(Color.DarkBlue);
                    embedBuilder.WithImageUrl("attachment://chart.png");

                    var users = new List<string>();

                    foreach (var user in configuration.Users
                                                      .OrderByDescending(obj => obj.Points)
                                                      .ThenBy(obj => obj.UserId))
                    {
                        if (user.UserId > 0)
                        {
                            var member = await commandContext.Guild
                                                             .GetUserAsync(user.UserId)
                                                             .ConfigureAwait(false);

                            users.Add($"{member.TryGetDisplayName()} ({user.Points:0.##})");
                        }
                    }

                    var chartConfiguration = new ChartConfigurationData
                                             {
                                                 Type = "bar",
                                                 Data = new Data.Json.QuickChart.Data
                                                        {
                                                            DataSets = [
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
                                                                       ],
                                                            Labels = users
                                                        },
                                                 Options = new OptionsCollection
                                                           {
                                                               Annotation = new AnnotationsCollection
                                                                            {
                                                                                Annotations = [
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
                                                                                              ]
                                                                            },
                                                               Scales = new ScalesCollection
                                                                        {
                                                                            XAxes = [
                                                                                        new XAxis
                                                                                        {
                                                                                            Ticks = new AxisTicks
                                                                                                    {
                                                                                                        FontColor = "#b3b3b3"
                                                                                                    }
                                                                                        }
                                                                                    ],
                                                                            YAxes = [
                                                                                        new YAxis
                                                                                        {
                                                                                            Ticks = new AxisTicks
                                                                                                    {
                                                                                                        FontColor = "#b3b3b3"
                                                                                                    }
                                                                                        }
                                                                                    ]
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
                        if (isFirstReply)
                        {
                            isFirstReply = false;

                            await commandContext.ReplyAsync(embed: embedBuilder.Build(), attachments: [new FileAttachment(chartStream, "chart.png")])
                                                .ConfigureAwait(false);
                        }
                        else
                        {
                            await commandContext.SendMessageAsync(embed: embedBuilder.Build(), attachments: [new FileAttachment(chartStream, "chart.png")])
                                                .ConfigureAwait(false);
                        }
                    }
                }
            }
        }
    }

    #endregion // Methods
}
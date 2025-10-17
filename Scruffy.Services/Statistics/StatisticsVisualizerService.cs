using Discord;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Statistics;
using Scruffy.Data.Json.QuickChart;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord.Interfaces;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Statistics;

/// <summary>
/// Visualizing statistics data
/// </summary>
public class StatisticsVisualizerService : LocatedServiceBase
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
    public StatisticsVisualizerService(LocalizationService localizationService, QuickChartConnector quickChartConnector)
        : base(localizationService)
    {
        _quickChartConnector = quickChartConnector;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Post personal overview
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PostMeOverview(IContextContainer commandContext)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var now = DateTime.Now;

            var embedBuilder = new EmbedBuilder().WithThumbnailUrl(commandContext.User.GetAvatarUrl())
                                                 .WithTitle(LocalizationGroup.GetText("MeOverviewTitle", "Personal Statistics"))
                                                 .WithColor(Color.Green)
                                                 .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                                 .WithTimestamp(now);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{commandContext.User.Mention} ({commandContext.User.Username}#{commandContext.User.Discriminator})");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(LocalizationGroup.GetText("MeOverviewDescription", "Personal statistics of the last 60 Days."));
            stringBuilder.AppendLine();

            embedBuilder.WithDescription(stringBuilder.ToString());

            stringBuilder = new StringBuilder();
            stringBuilder.Append(LocalizationGroup.GetText("MeOverviewJoined", "Joined:"));
            stringBuilder.Append(' ');
            stringBuilder.Append(Format.Code(commandContext.Member.JoinedAt?.LocalDateTime.ToString("g")));
            stringBuilder.Append(Environment.NewLine);

            stringBuilder.Append(LocalizationGroup.GetText("MeOverviewCreated", "Created:"));
            stringBuilder.Append(' ');
            stringBuilder.Append(Format.Code(commandContext.User.CreatedAt.LocalDateTime.ToString("g")));
            stringBuilder.Append(Environment.NewLine);

            stringBuilder.Append(LocalizationGroup.GetText("MeOverviewUserId", "User ID:"));
            stringBuilder.Append(' ');
            stringBuilder.Append(Format.Code(commandContext.User.Id.ToString()));
            stringBuilder.Append(Environment.NewLine);

            embedBuilder.AddField(LocalizationGroup.GetText("MeOverviewUserInfo", "User Info"), stringBuilder.ToString());

            stringBuilder = new StringBuilder();

            var mostActive = dbFactory.GetRepository<DiscordMessageRepository>()
                                      .GetQuery()
                                      .Where(obj => obj.DiscordServerId == commandContext.Guild.Id
                                                 && obj.DiscordAccountId == commandContext.User.Id)
                                      .GroupBy(obj => obj.DiscordChannelId)
                                      .Select(obj => new
                                                     {
                                                         Channeld = obj.Key,
                                                         Count = obj.Count()
                                                     })
                                      .OrderByDescending(obj => obj.Count)
                                      .FirstOrDefault();

            if (mostActive != null)
            {
                var channel = await commandContext.Guild
                                                  .GetChannelAsync(mostActive.Channeld)
                                                  .ConfigureAwait(false);

                if (channel is ITextChannel textChannel)
                {
                    stringBuilder.Append(LocalizationGroup.GetText("MeOverviewMessage", "Message:"));
                    stringBuilder.Append(' ');
                    stringBuilder.Append(textChannel.Mention);
                    stringBuilder.Append(' ');
                    stringBuilder.Append(Format.Code(mostActive.Count.ToString()
                                                   + ' '
                                                   + LocalizationGroup.GetText("MeOverviewMessages", "messages")));
                }
            }

            stringBuilder.Append('\u200b');

            embedBuilder.AddField(LocalizationGroup.GetText("MeOverviewMostActive", "Most Active Channel"), stringBuilder.ToString());

            stringBuilder = new StringBuilder();

            stringBuilder.Append(LocalizationGroup.GetText("MeOverview60Days", "60 Days:"));
            stringBuilder.Append(' ');
            var limit = now.AddDays(-60);
            stringBuilder.Append(Format.Code(dbFactory.GetRepository<DiscordMessageRepository>()
                                                      .GetQuery()
                                                      .Count(obj => obj.DiscordServerId == commandContext.Guild.Id
                                                                 && obj.DiscordAccountId == commandContext.User.Id
                                                                 && obj.TimeStamp > limit)
                                                      .ToString()
                                                    + ' '
                                                    + LocalizationGroup.GetText("MeOverviewMessages", "messages")));
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(LocalizationGroup.GetText("MeOverview7Days", "7 Days:"));
            stringBuilder.Append(' ');
            limit = now.AddDays(-7);
            stringBuilder.Append(Format.Code(dbFactory.GetRepository<DiscordMessageRepository>()
                                                      .GetQuery()
                                                      .Count(obj => obj.DiscordServerId == commandContext.Guild.Id
                                                                 && obj.DiscordAccountId == commandContext.User.Id
                                                                 && obj.TimeStamp > limit)
                                                      .ToString()
                                           + ' '
                                           + LocalizationGroup.GetText("MeOverviewMessages", "messages")));
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(LocalizationGroup.GetText("MeOverview24Hours", "24 Hours:"));
            stringBuilder.Append(' ');
            limit = now.AddDays(-1);
            stringBuilder.Append(Format.Code(dbFactory.GetRepository<DiscordMessageRepository>()
                                                         .GetQuery()
                                                         .Count(obj => obj.DiscordServerId == commandContext.Guild.Id
                                                                    && obj.DiscordAccountId == commandContext.User.Id
                                                                    && obj.TimeStamp > limit)
                                                         .ToString()
                                              + ' '
                                              + LocalizationGroup.GetText("MeOverviewMessages", "messages")));
            stringBuilder.Append(Environment.NewLine);

            embedBuilder.AddField(LocalizationGroup.GetText("MeOverviewMessagesField", "Messages"), stringBuilder.ToString());

            var messageChannels = dbFactory.GetRepository<DiscordMessageRepository>()
                                           .GetQuery()
                                           .Where(obj => obj.DiscordServerId == commandContext.Guild.Id
                                                      && obj.DiscordAccountId == commandContext.User.Id)
                                           .GroupBy(obj => obj.DiscordChannelId)
                                           .Select(obj => new
                                                          {
                                                              Channeld = obj.Key,
                                                              Count = obj.Count()
                                                          })
                                           .OrderByDescending(obj => obj.Count)
                                           .ToList();

            var messagesSum = messageChannels.Sum(obj => obj.Count);
            var messagesData = new List<int>();
            var messagesLabels = new List<string>();
            var messagesOther = 0;

            foreach (var messageChannel in messageChannels)
            {
                if (messageChannel.Count / (double)messagesSum > 0.1)
                {
                    messagesData.Add(messageChannel.Count);

                    var channel = await commandContext.Guild
                                                      .GetChannelAsync(messageChannel.Channeld)
                                                      .ConfigureAwait(false);

                    messagesLabels.Add(channel?.Name);
                }
                else
                {
                    messagesOther += messageChannel.Count;
                }
            }

            if (messagesOther > 0)
            {
                messagesData.Add(messagesOther);
                messagesLabels.Add(LocalizationGroup.GetText("MeOverviewOtherMessages", "Other"));
            }

            var chartConfiguration = new ChartConfigurationData
                                     {
                                         Type = "outlabeledDoughnut",
                                         Data = new Data.Json.QuickChart.Data
                                                {
                                                    DataSets = [
                                                                   new DataSet<int>
                                                                   {
                                                                       BorderColor = "#333333",
                                                                       BackgroundColor = [
                                                                                             "#0d1c26",
                                                                                             "#142b39",
                                                                                             "#1d3e53",
                                                                                             "#21475e",
                                                                                             "#2e6384",
                                                                                             "#357197",
                                                                                             "#3c80aa",
                                                                                             "#428ebd",
                                                                                             "#5599c3",
                                                                                             "#68a4ca"
                                                                                         ],
                                                                       Data = messagesData
                                                                   }
                                                               ],
                                                    Labels = messagesLabels
                                                },
                                         Options = new OptionsCollection
                                                   {
                                                       Plugins = new PluginsCollection
                                                                 {
                                                                     Legend = false,
                                                                     OutLabels = new OutLabelsCollection
                                                                                 {
                                                                                     Text = "%l (%v)",
                                                                                     Stretch = 40
                                                                                 },
                                                                     DoughnutLabel = new DoughnutLabelCollection
                                                                                     {
                                                                                         Labels = [
                                                                                                      new Label
                                                                                                      {
                                                                                                          Color = "white",
                                                                                                          Text = messagesSum.ToString()
                                                                                                      },
                                                                                                      new Label
                                                                                                      {
                                                                                                          Color = "white",
                                                                                                          Text = LocalizationGroup.GetText("MeOverviewMessages", "messages")
                                                                                                      },
                                                                                                  ]
                                                                                     },
                                                                 },
                                                       Title = new TitleConfiguration
                                                               {
                                                                   Display = true,
                                                                   FontColor = "white",
                                                                   FontSize = 30,
                                                                   Text = LocalizationGroup.GetText("MeOverviewMessagesField", "Messages")
                                                               }
                                                   }
                                     };

            var chartStream = await _quickChartConnector.GetChartAsStream(new ChartData
                                                                          {
                                                                              Width = 400,
                                                                              Height = 400,
                                                                              BackgroundColor = "#2f3136",
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
                embedBuilder.WithImageUrl("attachment://chart.png");

                await commandContext.ReplyAsync(embed: embedBuilder.Build(),
                                                attachments: [new FileAttachment(chartStream, "chart.png")])
                                    .ConfigureAwait(false);
            }
        }
    }

    #endregion // Methods
}
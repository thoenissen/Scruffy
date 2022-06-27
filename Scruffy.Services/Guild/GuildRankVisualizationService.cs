using Discord;
using Discord.WebSocket;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.Guild;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Data.Json.QuickChart;
using Scruffy.Data.Services.Guild;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Extensions;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Guild;

/// <summary>
/// Guild rank visualization service
/// </summary>
public class GuildRankVisualizationService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Lock (Accessing <see cref="_overviews"/>)
    /// </summary>
    private static LockFactory _overviewsLock = new();

    /// <summary>
    /// Lock (Modifying the message)
    /// </summary>
    private static LockFactory _modifyLock = new();

    /// <summary>
    /// Guild overviews
    /// </summary>
    private static Dictionary<ulong, GuildRankingOverviewData> _overviews = new();

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _dbFactory;

    /// <summary>
    /// QuickChart.io connector
    /// </summary>
    private readonly QuickChartConnector _quickChartConnector;

    /// <summary>
    /// User management service
    /// </summary>
    private readonly UserManagementService _userManagementService;

    /// <summary>
    /// Discord client
    /// </summary>
    private readonly DiscordSocketClient _discordClient;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="dbFactory">Repository factory</param>
    /// <param name="quickChartConnector">QuickChart.io connector</param>
    /// <param name="userManagementService">User management service</param>
    /// <param name="discordClient">Discord client</param>
    public GuildRankVisualizationService(LocalizationService localizationService,
                                         RepositoryFactory dbFactory,
                                         QuickChartConnector quickChartConnector,
                                         UserManagementService userManagementService,
                                         DiscordSocketClient discordClient)
        : base(localizationService)
    {
        _dbFactory = dbFactory;
        _quickChartConnector = quickChartConnector;
        _userManagementService = userManagementService;
        _discordClient = discordClient;
    }

    #endregion // Constructor

    #region Public methods

    /// <summary>
    /// Post overview
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="messageId">Optional message id which will be refreshed</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PostOverview(InteractionContextContainer context, ulong? messageId)
    {
        if (messageId == null)
        {
            var message = await context.DeferProcessing()
                                       .ConfigureAwait(false);

            messageId = message.Id;
        }

        var data = await GetOverviewData(context.Guild.Id, false).ConfigureAwait(false);

        await RefreshOverviewMessage(data, 0, context.Channel.Id, messageId.Value).ConfigureAwait(false);
    }

    /// <summary>
    /// Refresh existing overview
    /// </summary>
    /// <param name="discordServerId">Id of the Discord server</param>
    /// <param name="channelId">Id of the channel</param>
    /// <param name="messageId">Id of the message</param>
    /// <param name="page">Page</param>
    /// <param name="isForceRefresh">Force refresh data?</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RefreshOverview(ulong discordServerId, ulong channelId, ulong messageId, int page, bool isForceRefresh)
    {
        var data = await GetOverviewData(discordServerId, isForceRefresh).ConfigureAwait(false);

        await RefreshOverviewMessage(data, page, channelId, messageId).ConfigureAwait(false);
    }

    /// <summary>
    /// Post personal overview
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="guildUser">User</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PostPersonalOverview(InteractionContextContainer context, IGuildUser guildUser)
    {
        await context.DeferProcessing()
                     .ConfigureAwait(false);

        var user = await _userManagementService.GetUserByDiscordAccountId(guildUser.Id)
                                               .ConfigureAwait(false);

        var limit = DateTime.Today.AddDays(-63);
        var today = DateTime.Today;

        var guildMemberSubQuery = _dbFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                            .GetQuery()
                                            .Select(obj => obj);

        var guildMemberQuery = _dbFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                         .GetQuery()
                                         .Where(obj => guildMemberSubQuery.Any(obj2 => obj2.GuildId == obj.GuildId
                                                                                    && obj2.Date > obj.Date) == false);
        var accountsQuery = _dbFactory.GetRepository<GuildWarsAccountRepository>()
                                      .GetQuery()
                                      .Select(obj => obj);

        var userPoints = _dbFactory.GetRepository<GuildRankCurrentPointsRepository>()
                                   .GetQuery()
                                   .Where(obj => obj.Date >= limit
                                              && obj.Date < today
                                              && obj.Guild.DiscordServerId == context.Guild.Id
                                              && obj.UserId == user.Id
                                              && accountsQuery.Any(obj2 => obj2.UserId == obj.UserId
                                                                        && guildMemberQuery.Any(obj3 => obj3.Name == obj2.Name
                                                                                                     && obj3.GuildId == obj.GuildId)))
                                   .GroupBy(obj => obj.Type)
                                   .Select(obj => new
                                                  {
                                                      Type = obj.Key,
                                                      Points = obj.Sum(obj2 => obj2.Points)
                                                  })
                                   .Where(obj => obj.Points != 0)
                                   .OrderByDescending(obj => obj.Points)
                                   .ToList();

        if (userPoints.Count > 0)
        {
            var summedPoints = userPoints.Sum(obj => obj.Points);

            var rank = _dbFactory.GetRepository<GuildRankCurrentPointsRepository>()
                                 .GetQuery()
                                 .Where(obj => obj.Date >= limit
                                            && obj.Date < today
                                            && obj.Guild.DiscordServerId == context.Guild.Id
                                            && obj.UserId != user.Id
                                            && accountsQuery.Any(obj2 => obj2.UserId == obj.UserId
                                                                      && guildMemberQuery.Any(obj3 => obj3.Name == obj2.Name
                                                                                                   && obj3.GuildId == obj.GuildId)))
                                 .GroupBy(obj => obj.UserId)
                                 .Select(obj => new
                                                {
                                                    UserId = obj.Key,
                                                    Points = obj.Sum(obj2 => obj2.Points)
                                                })
                                 .Count(obj => obj.Points > summedPoints)
                     + 1;

            var descriptionBuilder = new StringBuilder();

            descriptionBuilder.Append(LocalizationGroup.GetText("RankingUser", "User"));
            descriptionBuilder.Append(": ");
            descriptionBuilder.Append(guildUser.Mention);
            descriptionBuilder.Append(Environment.NewLine);

            descriptionBuilder.Append(LocalizationGroup.GetText("RankingRank", "Rank"));
            descriptionBuilder.Append(": ");
            descriptionBuilder.Append(rank);

            if (rank == 1)
            {
                descriptionBuilder.Append(' ');
                descriptionBuilder.Append(DiscordEmoteService.GetGuildWars2GoldEmote(context.Client));
            }
            else if (rank == 2)
            {
                descriptionBuilder.Append(' ');
                descriptionBuilder.Append(DiscordEmoteService.GetGuildWars2SilverEmote(context.Client));
            }
            else if (rank  == 3)
            {
                descriptionBuilder.Append(' ');
                descriptionBuilder.Append(DiscordEmoteService.GetGuildWars2CopperEmote(context.Client));
            }

            descriptionBuilder.Append(Environment.NewLine);

            descriptionBuilder.Append(LocalizationGroup.GetText("RankingPoints", "Points"));
            descriptionBuilder.Append(": ");
            descriptionBuilder.Append(summedPoints.ToString("0.00", LocalizationGroup.CultureInfo));
            descriptionBuilder.Append(Environment.NewLine);

            var embedBuilder = new EmbedBuilder()
                               .WithTitle($"{LocalizationGroup.GetText("RankingPersonalOverview", "Guild ranking personal points overview")}")
                               .WithDescription(descriptionBuilder.ToString())
                               .WithColor(Color.DarkBlue)
                               .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                               .WithTimestamp(DateTime.Now)
                               .WithImageUrl("attachment://chart.png");

            var chartConfiguration = new ChartConfigurationData
                                     {
                                         Type = "bar",
                                         Data = new Data.Json.QuickChart.Data
                                                {
                                                    DataSets = new List<DataSet>
                                                               {
                                                                   new DataSet<double>
                                                                   {
                                                                       BorderColor = "#333333",
                                                                       BackgroundColor = new List<string>
                                                                                         {
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
                                                                                         },
                                                                       Data = userPoints.Select(obj => obj.Points)
                                                                                        .ToList()
                                                                   }
                                                               },
                                                    Labels = userPoints.Select(obj => $"{LocalizationGroup.GetText(obj.Type.ToString(), obj.Type.ToString())} ({obj.Points.ToString("0.##", LocalizationGroup.CultureInfo)})")
                                                                       .ToList()
                                                },
                                         Options = new OptionsCollection
                                                   {
                                                       Plugins = new PluginsCollection
                                                                 {
                                                                     Legend = false
                                                                 },
                                                       Title = new TitleConfiguration
                                                               {
                                                                   Display = true,
                                                                   FontColor = "white",
                                                                   FontSize = 26,
                                                                   Text = LocalizationGroup.GetText("MeOverviewChartTitle", "Point distribution")
                                                               }
                                                   }
                                     };

            var chartStream = await _quickChartConnector.GetChartAsStream(new ChartData
                                                                          {
                                                                              Width = 600,
                                                                              Height = 500,
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

                await context.SendMessageAsync(embed: embedBuilder.Build(),
                                               attachments: new[] { new FileAttachment(chartStream, "chart.png") })
                             .ConfigureAwait(false);
            }
        }
        else
        {
            await context.SendMessageAsync(LocalizationGroup.GetText("NoPersonalData", "No ranking data is available."))
                         .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Post personal overview
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="guildUser">User</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PostPersonalHistoryTypeOverview(InteractionContextContainer context, IGuildUser guildUser)
    {
        await context.DeferProcessing()
                     .ConfigureAwait(false);

        var user = await _userManagementService.GetUserByDiscordAccountId(guildUser.Id)
                                               .ConfigureAwait(false);

        var from = DateTime.Today.AddDays(-63);
        var to = DateTime.Today.AddDays(-1);

        var currentPoints = _dbFactory.GetRepository<GuildRankCurrentPointsRepository>()
                                          .GetQuery()
                                          .Where(obj => obj.UserId == user.Id
                                                     && obj.Date >= from
                                                     && obj.Date <= to
                                                     && obj.Points != 0
                                                     && obj.Guild.DiscordServerId == context.Guild.Id)
                                          .Select(obj => new
                                                         {
                                                             obj.Date,
                                                             obj.Type,
                                                             obj.Points
                                                         })
                                          .ToList();

        if (currentPoints.Count > 0)
        {
            var dataSets = new List<DataSet>();

            var dates = Enumerable.Range(-63, 63)
                                  .Select(obj => DateTime.Today.AddDays(obj))
                                  .ToList();

            var colors = new List<string>
                         {
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
                         };

            foreach (var type in Enum.GetValues(typeof(GuildRankPointType))
                                     .OfType<GuildRankPointType>())
            {
                dataSets.Add(new DataSet<double>
                             {
                                 Label = LocalizationGroup.GetText(type.ToString(), type.ToString()),
                                 BorderColor = colors[(int)type],
                                 Data = dates.Select(obj => currentPoints.FirstOrDefault(obj2 => obj2.Date == obj
                                                                                              && obj2.Type == type)?.Points
                                                         ?? 0.0)
                                             .ToList(),
                                 Fill = false,
                                 PointRadius = 0.0,
                                 BorderDash = (int)type % 2  == 0 ?  new double[] { 5 } : null
                             });
            }

            var descriptionBuilder = new StringBuilder();

            descriptionBuilder.Append(LocalizationGroup.GetText("RankingUser", "User"));
            descriptionBuilder.Append(": ");
            descriptionBuilder.Append(guildUser.Mention);

            var embedBuilder = new EmbedBuilder().WithTitle($"{LocalizationGroup.GetText("RankingPersonalTypeHistoryOverview", "Guild ranking history per type")}")
                                                 .WithDescription(descriptionBuilder.ToString())
                                                 .WithColor(Color.DarkBlue)
                                                 .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                                 .WithTimestamp(DateTime.Now)
                                                 .WithImageUrl("attachment://chart.png");

            var chartConfiguration = new ChartConfigurationData
                                     {
                                         Type = "line",
                                         Data = new Data.Json.QuickChart.Data
                                                {
                                                    Labels = dates.Select(obj => obj.ToString("d", LocalizationGroup.CultureInfo))
                                                                  .ToList(),
                                                    DataSets = dataSets
                                                },
                                         Options = new OptionsCollection
                                                   {
                                                       Legend = new ChartLegendConfiguration
                                                                {
                                                                    Position = "bottom"
                                                                },
                                                       Title = new TitleConfiguration
                                                               {
                                                                   Display = true,
                                                                   FontColor = "white",
                                                                   FontSize = 26,
                                                                   Text = LocalizationGroup.GetText("MeOverviewChartTitle", "Point distribution")
                                                               }
                                                   }
                                     };

            var chartStream = await _quickChartConnector.GetChartAsStream(new ChartData
                                                                          {
                                                                              Width = 600,
                                                                              Height = 600,
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

                await context.SendMessageAsync(embed: embedBuilder.Build(),
                                               attachments: new[] { new FileAttachment(chartStream, "chart.png") })
                             .ConfigureAwait(false);
            }
        }
        else
        {
            await context.SendMessageAsync(LocalizationGroup.GetText("NoPersonalData", "No ranking data is available."))
                         .ConfigureAwait(false);
        }
    }

    #endregion // Public methods

    #region Private methods

    /// <summary>
    /// Determinate overview data
    /// </summary>
    /// <param name="discordServerId">Discord server id</param>
    /// <param name="isForceRefresh">Force to refresh the cached data?</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task<GuildRankingOverviewData> GetOverviewData(ulong discordServerId, bool isForceRefresh)
    {
        GuildRankingOverviewData data = null;

        var scopeLock = await _overviewsLock.CreateLockAsync()
                                   .ConfigureAwait(false);
        await using (scopeLock.ConfigureAwait(false))
        {
            if (isForceRefresh
             || _overviews.TryGetValue(discordServerId, out data) == false)
            {
                data = new GuildRankingOverviewData();

                var limit = DateTime.Today.AddDays(-63);
                var today = DateTime.Today;

                var guildMemberSubQuery = _dbFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                                    .GetQuery()
                                                    .Select(obj => obj);

                var guildMemberQuery = _dbFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                                 .GetQuery()
                                                 .Where(obj => guildMemberSubQuery.Any(obj2 => obj2.GuildId == obj.GuildId
                                                                                            && obj2.Date > obj.Date) == false);
                var accountsQuery = _dbFactory.GetRepository<GuildWarsAccountRepository>()
                                              .GetQuery()
                                              .Select(obj => obj);

                var discordUsersQuery = _dbFactory.GetRepository<DiscordAccountRepository>()
                                                  .GetQuery()
                                                  .Select(obj => obj);

                var rankAssignments = _dbFactory.GetRepository<GuildRankAssignmentRepository>()
                                                .GetQuery()
                                                .Select(obj => obj);

                var users = await _dbFactory.GetRepository<GuildRankCurrentPointsRepository>()
                                            .GetQuery()
                                            .Where(obj => obj.Date >= limit
                                                       && obj.Date < today
                                                       && obj.Guild.DiscordServerId == discordServerId
                                                       && accountsQuery.Any(obj2 => obj2.UserId == obj.UserId
                                                                                 && guildMemberQuery.Any(obj3 => obj3.Name == obj2.Name
                                                                                                              && obj3.GuildId == obj.GuildId)))
                                            .GroupBy(obj => obj.UserId)
                                            .Select(obj => new OverviewUserPointsData
                                                           {
                                                               UserId = obj.Key,
                                                               Points = obj.Sum(obj2 => obj2.Points),
                                                               DiscordUserId = discordUsersQuery.Where(obj2 => obj2.UserId == obj.Key)
                                                                                                .Select(obj2 => (ulong?)obj2.Id)
                                                                                                .FirstOrDefault(),
                                                               DiscordRoleId = rankAssignments.Where(obj2 => obj2.UserId == obj.Key)
                                                                                              .Select(obj2 => (ulong?)obj2.Rank.DiscordRoleId)
                                                                                              .FirstOrDefault()
                                                           })
                                            .OrderByDescending(obj => obj.Points)
                                            .ToListAsync()
                                            .ConfigureAwait(false);

                var guild = _discordClient.GetGuild(discordServerId);
                if (guild != null)
                {
                    foreach (var user in users)
                    {
                        if (user.DiscordUserId != null)
                        {
                            var userName = "Invalid user";
                            try
                            {
                                var member = guild.GetUser(user.DiscordUserId.Value);
                                if (member != null)
                                {
                                    userName = $"{member.TryGetDisplayName()}";
                                }
                            }
                            catch
                            {
                            }

                            user.UserName = $"{userName} [{user.Points:0.00}]";
                        }

                        if (user.DiscordRoleId != null)
                        {
                            var role = guild.GetRole(user.DiscordRoleId.Value);
                            if (role != null)
                            {
                                user.UserColor = role.Color.ToString();
                            }
                        }
                    }
                }

                var page = new List<OverviewUserPointsData>();
                data.Pages = new List<List<OverviewUserPointsData>>
                             {
                                 page
                             };

                foreach (var user in users)
                {
                    if (page.Count == 20)
                    {
                        page = new List<OverviewUserPointsData>();
                        data.Pages.Add(page);
                    }

                    page.Add(user);
                }

                data.UserCount = users.Count;

                _overviews[discordServerId] = data;
            }
        }

        return data;
    }

    /// <summary>
    /// Refresh overview message
    /// </summary>
    /// <param name="data">Data</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="channelId">Id of the channel</param>
    /// <param name="messageId">Id of the message</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task RefreshOverviewMessage(GuildRankingOverviewData data, int pageNumber, ulong channelId, ulong messageId)
    {
        var componentsBuilder = new ComponentBuilder();

        if (pageNumber >= data.Pages.Count)
        {
            pageNumber = data.Pages.Count - 1;
        }

        if (pageNumber < 0)
        {
            pageNumber = 0;
        }

        componentsBuilder.WithButton(null, "guild;navigate_to_page_guild_ranking;0;first", ButtonStyle.Secondary, DiscordEmoteService.GetFirstEmote(_discordClient), null, pageNumber == 0);
        componentsBuilder.WithButton(null, $"guild;navigate_to_page_guild_ranking;{pageNumber - 1};previous", ButtonStyle.Secondary, DiscordEmoteService.GePreviousEmote(_discordClient), null, pageNumber - 1 < 0);
        componentsBuilder.WithButton(null, $"guild;navigate_to_page_guild_ranking;{pageNumber + 1};next", ButtonStyle.Secondary, DiscordEmoteService.GetNextEmote(_discordClient), null, pageNumber + 1 >= data.Pages.Count);
        componentsBuilder.WithButton(null, $"guild;navigate_to_page_guild_ranking;{data.Pages.Count - 1};last", ButtonStyle.Secondary, DiscordEmoteService.GetLastEmote(_discordClient), null, pageNumber + 1 >= data.Pages.Count);

        var descriptionBuilder = new StringBuilder();

        descriptionBuilder.Append(LocalizationGroup.GetText("RankingUserCount", "User count"));
        descriptionBuilder.Append(": ");
        descriptionBuilder.Append(data.UserCount);
        descriptionBuilder.Append(Environment.NewLine);

        var description = descriptionBuilder.ToString();

        var embedBuilder = new EmbedBuilder().WithTitle(LocalizationGroup.GetText("RankingOverview", "Guild ranking points overview"))
                                             .WithDescription(description)
                                             .WithColor(Color.DarkBlue)
                                             .WithFooter(LocalizationGroup.GetFormattedText("PageFooter", "Page {0} of {1}", pageNumber + 1, data.Pages.Count), "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                             .WithTimestamp(DateTime.Now)
                                             .WithImageUrl("attachment://chart.png");

        var page = data.Pages[pageNumber];

        var minValue = page.Any() ? page.Min(obj => obj.Points) : 0;

        minValue = minValue > 0
                       ? 0
                       : -10 * (((int)Math.Ceiling(minValue * -1) / 10) + 1);

        var maxValue = (((int)Math.Ceiling(page.Any() ? page.Max(obj => obj.Points) : 0) / 10) + 1) * 10;

        var chartConfiguration = new ChartConfigurationData
                                 {
                                     Type = "horizontalBar",
                                     Data = new Data.Json.QuickChart.Data
                                            {
                                                DataSets = new List<DataSet>
                                                           {
                                                               new DataSet<double>
                                                               {
                                                                   BackgroundColor = page.Select(obj => obj.UserColor ?? "#98A4A6")
                                                                                         .ToList(),
                                                                   BorderColor = "#274d85",
                                                                   Data = page.Select(obj => obj.Points)
                                                                              .ToList()
                                                               }
                                                           },
                                                Labels = page.Select(obj => obj.UserName)
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
                                                                                Ticks = new AxisTicks<double>
                                                                                        {
                                                                                            MinValue = minValue,
                                                                                            MaxValue = maxValue,
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
                                                                          Height = (20 * page.Count) + 40,
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
            if (_discordClient.GetChannel(channelId) is ITextChannel channel)
            {
                if (await channel.GetMessageAsync(messageId).ConfigureAwait(false) is IUserMessage message)
                {
                    var scopeLock = await _modifyLock.CreateLockAsync()
                                                     .ConfigureAwait(false);
                    await using (scopeLock.ConfigureAwait(false))
                    {
                        try
                        {
                            await message.ModifyAsync(obj =>
                                                      {
                                                          obj.Content = null;
                                                          obj.Embed = embedBuilder.Build();
                                                          obj.Components = componentsBuilder.Build();
                                                          obj.Attachments = new[] { new FileAttachment(chartStream, "chart.png") };
                                                      })
                                         .ConfigureAwait(false);
                        }
                        catch (ObjectDisposedException)
                        {
                            // TODO Reproduce error
                            // Sometime if multiple buttons are executed the internal stream gets disposed
                        }
                    }
                }
            }
        }
    }

    #endregion // Private methods
}
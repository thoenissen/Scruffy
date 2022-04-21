using Discord;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.Guild;
using Scruffy.Data.Json.QuickChart;
using Scruffy.Data.Services.Guild;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Exceptions;
using Scruffy.Services.Core.Extensions;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Interfaces;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Guild;

/// <summary>
/// Guild rank visualization service
/// </summary>
public class GuildRankVisualizationService : LocatedServiceBase
{
    #region Fields

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

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="dbFactory">Repository factory</param>
    /// <param name="quickChartConnector">QuickChart.io connector</param>
    /// <param name="userManagementService">User management service</param>
    public GuildRankVisualizationService(LocalizationService localizationService,
                                         RepositoryFactory dbFactory,
                                         QuickChartConnector quickChartConnector,
                                         UserManagementService userManagementService)
        : base(localizationService)
    {
        _dbFactory = dbFactory;
        _quickChartConnector = quickChartConnector;
        _userManagementService = userManagementService;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Post overview
    /// </summary>
    /// <param name="context">Context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PostOverview(IContextContainer context)
    {
        var userPoints = await GetOverviewUsers(context.Guild.Id).ConfigureAwait(false);

        foreach (var user in userPoints)
        {
            if (user.DiscordUserId != null)
            {
                var member = await context.Guild
                                          .GetUserAsync(user.DiscordUserId.Value)
                                          .ConfigureAwait(false);
                if (member != null)
                {
                    user.UserName = $"{member.TryGetDisplayName()} [{user.Points:0.00}]";
                }
            }

            if (user.DiscordRoleId != null)
            {
                var role = context.Guild.Roles.FirstOrDefault(obj => obj.Id == user.DiscordRoleId);
                if (role != null)
                {
                    user.UserColor = role.Color.ToString();
                }
            }
        }

        var descriptionBuilder = new StringBuilder();

        descriptionBuilder.Append(LocalizationGroup.GetText("RankingUserCount", "User count"));
        descriptionBuilder.Append(": ");
        descriptionBuilder.Append(userPoints.Count);
        descriptionBuilder.Append(Environment.NewLine);

        var description = descriptionBuilder.ToString();

        var page = new List<OverviewUserPointsData>();
        var pages = new List<List<OverviewUserPointsData>>
                    {
                        page
                    };

        foreach (var user in userPoints)
        {
            if (page.Count == 20)
            {
                page = new List<OverviewUserPointsData>();
                pages.Add(page);
            }

            page.Add(user);
        }

        var continueLoop = true;
        var currentPage = 0;
        ulong? messageId = null;

        while (continueLoop)
        {
            try
            {
                var components = context.Interactivity.CreateTemporaryComponentContainer<int>(obj => obj.User.Id == context.User.Id);
                await using (components.ConfigureAwait(false))
                {
                    var componentsBuilder = new ComponentBuilder();

                    componentsBuilder.WithButton(null, components.AddButton(1), ButtonStyle.Secondary, DiscordEmoteService.GetFirstEmote(context.Client), null, currentPage == 0);
                    componentsBuilder.WithButton(null, components.AddButton(2), ButtonStyle.Secondary, DiscordEmoteService.GePreviousEmote(context.Client), null, currentPage - 1 < 0);
                    componentsBuilder.WithButton(null, components.AddButton(3), ButtonStyle.Secondary, DiscordEmoteService.GetNextEmote(context.Client), null, currentPage + 1 >= pages.Count);
                    componentsBuilder.WithButton(null, components.AddButton(4), ButtonStyle.Secondary, DiscordEmoteService.GetLastEmote(context.Client), null, currentPage + 1 >= pages.Count);

                    messageId = await PostOverviewMessage(context, description, pages[currentPage], currentPage + 1, pages.Count, componentsBuilder.Build(), messageId).ConfigureAwait(false);

                    components.StartTimeout();

                    var (component, identification) = await components.Task
                                                                      .ConfigureAwait(false);

                    await component.DeferAsync()
                                   .ConfigureAwait(false);

                    switch (identification)
                    {
                        case 1:
                            {
                                currentPage = 0;
                            }
                            break;
                        case 2:
                            {
                                currentPage--;
                            }
                            break;
                        case 3:
                            {
                                currentPage++;
                            }
                            break;
                        case 4:
                            {
                                currentPage = pages.Count - 1;
                            }
                            break;
                    }

                    if (currentPage < 0)
                    {
                        currentPage = 0;
                    }
                    else if (currentPage > pages.Count - 1)
                    {
                        currentPage = page.Count - 1;
                    }
                }
            }
            catch (ScruffyTimeoutException)
            {
                if (messageId != null)
                {
                    await context.Channel
                                 .ModifyMessageAsync(messageId.Value, obj => obj.Components = new ComponentBuilder().Build())
                                 .ConfigureAwait(false);
                }

                continueLoop = false;
            }
        }
    }

    /// <summary>
    /// Post personal overview
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="guildUser">User</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PostPersonalOverview(IContextContainer context, IGuildUser guildUser)
    {
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

            await context.Channel
                         .SendFileAsync(new FileAttachment(chartStream, "chart.png"),
                                        embed: embedBuilder.Build())
                         .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Determination of the current guild ranking
    /// </summary>
    /// <param name="discordServerId">Id of the discord server</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private Task<List<OverviewUserPointsData>> GetOverviewUsers(ulong discordServerId)
    {
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

        return _dbFactory.GetRepository<GuildRankCurrentPointsRepository>()
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
                         .ToListAsync();
    }

    /// <summary>
    /// Post overview message
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="description">Description</param>
    /// <param name="page">Page data</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageCount">Page count</param>
    /// <param name="buttons">Buttons</param>
    /// <param name="messageId">Message id</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task<ulong> PostOverviewMessage(IContextContainer context, string description, List<OverviewUserPointsData> page, int pageNumber, int pageCount, MessageComponent buttons, ulong? messageId)
    {
        var embedBuilder = new EmbedBuilder().WithTitle(LocalizationGroup.GetText("RankingOverview", "Guild ranking points overview"))
                                             .WithDescription(description)
                                             .WithColor(Color.DarkBlue)
                                             .WithFooter(LocalizationGroup.GetFormattedText("PageFooter", "Page {0} of {1}", pageNumber, pageCount), "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                             .WithTimestamp(DateTime.Now)
                                             .WithImageUrl("attachment://chart.png");

        var minValue = page.Min(obj => obj.Points);

        minValue = minValue > 0
                       ? 0
                       : -10 * (((int)Math.Ceiling(minValue * -1) / 10) + 1);

        var maxValue = (((int)Math.Ceiling(page.Max(obj => obj.Points)) / 10) + 1) * 10;

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
                                                                          Height = 20 * page.Count,
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
            if (messageId != null)
            {
                await context.Channel
                             .ModifyMessageAsync(messageId.Value,
                                                 obj =>
                                                 {
                                                     obj.Attachments = new[] { new FileAttachment(chartStream, "chart.png") };
                                                     obj.Embed = embedBuilder.Build();
                                                     obj.Components = buttons;
                                                 })
                             .ConfigureAwait(false);

                return messageId.Value;
            }

            return (await context.Channel
                                 .SendFileAsync(new FileAttachment(chartStream, "chart.png"), embed: embedBuilder.Build(), components: buttons)
                                 .ConfigureAwait(false)).Id;
        }
    }

    #endregion // Methods
}
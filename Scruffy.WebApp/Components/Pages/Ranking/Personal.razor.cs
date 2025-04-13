using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.Guild;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Data.Json.ChartJs;
using Scruffy.Data.Services.Guild;

namespace Scruffy.WebApp.Components.Pages.Ranking;

/// <summary>
/// Personal ranking page
/// </summary>
[Authorize(Roles = "Member")]
public partial class Personal
{
    #region Fields

    /// <summary>
    /// Chart options
    /// </summary>
    private readonly ChartOptions _chartOptions;

    /// <summary>
    /// Chart data
    /// </summary>
    private ChartData _chartData;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public Personal()
    {
        _chartOptions = new ChartOptions
                        {
                            Responsive = true,
                            MaintainAspectRatio = false,
                            Plugins = new PluginsCollection
                                      {
                                          Legend = false
                                      },
                            Scales = new Scales
                                     {
                                         X = new Axes
                                             {
                                                 Grid = new GridConfiguration
                                                        {
                                                            Color = "lightgrey"
                                                        },
                                                 Ticks = new AxisTicks
                                                         {
                                                             Color = "#A0AEC0"
                                                         }
                                             },
                                         Y = new Axes
                                             {
                                                 Grid = new GridConfiguration
                                                        {
                                                            Color = "lightgrey"
                                                        },
                                                 Ticks = new AxisTicks
                                                         {
                                                             Color = "#A0AEC0"
                                                         }
                                             }
                                     }
                        };
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Authentication state provider
    /// </summary>
    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Get description
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns>Description</returns>
    private string GetDescription(GuildRankPointType type)
    {
        return type switch
               {
                   GuildRankPointType.Login => "Login",
                   GuildRankPointType.Representation => "Representation",
                   GuildRankPointType.AchievementPoints => "Achievement points",
                   GuildRankPointType.Membership => "Membership",
                   GuildRankPointType.Donation => "Donations",
                   GuildRankPointType.DiscordVoiceActivity => "Discord voice activity",
                   GuildRankPointType.DiscordMessageActivity => "Discord message activity",
                   GuildRankPointType.Events => "Events",
                   GuildRankPointType.Development => "Development",
                   _ => type.ToString()
               };
    }

    #endregion // Methods

    #region ComponentBase

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync().ConfigureAwait(false);

        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(false);

        var nameIdentifier = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(nameIdentifier) == false
            && int.TryParse(nameIdentifier, out var userId))
        {
            var limit = DateTime.Today.AddDays(-63);
            var today = DateTime.Today;

            using (var repositoryFactory = RepositoryFactory.CreateInstance())
            {
                var guildMemberSubQuery = repositoryFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                                           .GetQuery();

                var guildMemberQuery = repositoryFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                                        .GetQuery()
                                                        .Where(member => guildMemberSubQuery.Any(checkMember => checkMember.GuildId == member.GuildId
                                                                                                 && checkMember.Date > member.Date) == false);
                var accountsQuery = repositoryFactory.GetRepository<GuildWarsAccountRepository>()
                                                     .GetQuery();

                var userConfiguration = repositoryFactory.GetRepository<GuildUserConfigurationRepository>()
                                                         .GetQuery();

                var userPoints = repositoryFactory.GetRepository<GuildRankCurrentPointsRepository>()
                                                  .GetQuery()
                                                  .Where(rankPoint => rankPoint.Date >= limit
                                                                      && rankPoint.Date < today
                                                                      && rankPoint.Guild.DiscordServerId == WebAppConfiguration.DiscordServerId
                                                                      && rankPoint.UserId == userId
                                                                      && accountsQuery.Any(account => account.UserId == rankPoint.UserId
                                                                                                      && guildMemberQuery.Any(member => member.Name == account.Name
                                                                                                                              && member.GuildId == rankPoint.GuildId))
                                                                      && userConfiguration.Any(configuration => configuration.UserId == rankPoint.UserId
                                                                                                                && configuration.GuildId == rankPoint.GuildId
                                                                                                                && configuration.IsInactive) == false)
                                                  .GroupBy(rankPoint => rankPoint.Type)
                                                  .Select(rankPoint => new GuildRankUserPointsData
                                                                       {
                                                                           Type = rankPoint.Key,
                                                                           Points = rankPoint.Sum(obj2 => obj2.Points)
                                                                       })
                                                  .Where(rankPoint => rankPoint.Points != 0)
                                                  .OrderByDescending(rankPoint => rankPoint.Points)
                                                  .ToList();

                if (userPoints.Count > 0)
                {
                    _chartData = new ChartData
                                 {
                                     Datasets = [
                                                    new DataSet
                                                    {
                                                        BackgroundColor = ["#357197"],
                                                        Data = userPoints.Select(obj => obj.Points)
                                                                         .ToArray()
                                                    }
                                                ],
                                     Labels = userPoints.Select(obj => $"{GetDescription(obj.Type)} ({obj.Points:0.##})").ToArray()
                                 };
                }
            }
        }
    }

    #endregion // ComponentBase
}
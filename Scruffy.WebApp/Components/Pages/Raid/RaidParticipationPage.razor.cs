using System.Drawing;
using System.Linq;

using Microsoft.AspNetCore.Authorization;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Json.ChartJs;

namespace Scruffy.WebApp.Components.Pages.Raid;

/// <summary>
/// Participation chart
/// </summary>
[Authorize(Roles = "Member")]
public partial class RaidParticipationPage
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
    public RaidParticipationPage()
    {
        _chartOptions = new ChartOptions
                        {
                            Responsive = true,
                            MaintainAspectRatio = false,
                            Plugins = new PluginsCollection(),
                            IndexAxis = "y",
                            Scales = new Scales
                                     {
                                         X = new Axes
                                             {
                                                 Grid = new GridConfiguration
                                                        {
                                                            Color = WebAppConfiguration.Colors.Text
                                                        },
                                                 Ticks = new AxisTicks
                                                         {
                                                             Color = WebAppConfiguration.Colors.Text
                                                         }
                                             },
                                         Y = new Axes
                                             {
                                                 Ticks = new AxisTicks
                                                         {
                                                             Color = WebAppConfiguration.Colors.Text
                                                         }
                                             }
                                     }
                        };
    }

    #endregion // Constructor

    #region ComponentBase

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var users = dbFactory.GetRepository<RaidCurrentUserPointsRepository>()
                                 .GetQuery()
                                 .Where(user => user.User.DiscordAccounts.Any(account => account.Members.Any(member => member.ServerId == WebAppConfiguration.DiscordServerId)))
                                 .Select(user => new
                                                 {
                                                     Name = user.User
                                                                .DiscordAccounts
                                                                .Select(account => account.Members.Where(member => member.ServerId == WebAppConfiguration.DiscordServerId)
                                                                                                  .Select(member => member.Name)
                                                                                                  .FirstOrDefault())
                                                                .FirstOrDefault(),
                                                     Points = user.Points * 100.0
                                                 })
                                 .OrderByDescending(obj => obj.Points)
                                 .ToList();

            _chartData = new ChartData
                         {
                             Datasets = [
                                            new DataSet
                                            {
                                                Color = users.Select(_ => string.Empty).ToArray(),
                                                BackgroundColor = users.Select(_ => WebAppConfiguration.Colors.Secondary).ToArray(),
                                                Data = users.Select(user => user.Points).ToArray()
                                            }
                                        ],
                             Labels = users.Select(user => user.Name).ToArray()
                         };
        }
    }

    #endregion // ComponentBase
}
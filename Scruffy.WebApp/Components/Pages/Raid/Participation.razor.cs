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
public partial class Participation
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
    public Participation()
    {
        _chartOptions = new ChartOptions
                        {
                            Responsive = true,
                            MaintainAspectRatio = false,
                            Plugins = new PluginsCollection
                                      {
                                          Legend = false
                                      },
                            IndexAxis = "y",
                            Scales = new Scales
                                     {
                                         X = new Axes
                                             {
                                                 Grid = new GridConfiguration
                                                        {
                                                            Color = "lightgrey"
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
                                 .OrderByDescending(obj => obj.Points)
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
                                 .ToList();

            _chartData = new ChartData
                         {
                             Datasets = [
                                            new DataSet
                                            {
                                                BackgroundColor = users.Select(_ => "#316ed5").ToArray(),
                                                Data = users.Select(user => user.Points).ToArray()
                                            }
                                        ],
                             Labels = users.Select(user => user.Name).ToArray()
                         };
        }
    }

    #endregion // ComponentBase
}
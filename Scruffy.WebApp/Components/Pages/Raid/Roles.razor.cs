using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Authorization;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.WebApp.DTOs.Raid;

namespace Scruffy.WebApp.Components.Pages.Raid;

/// <summary>
/// User raid roles
/// </summary>
[Authorize(Roles = "Member")]
public partial class Roles
{
    #region Fields

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory = RepositoryFactory.CreateInstance();

    /// <summary>
    /// List of user raid roles
    /// </summary>
    private List<UserRaidRoleDTO> _userRaidRoles;

    #endregion // Fields

    #region ComponentBase

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        var raidRoles = _repositoryFactory.GetRepository<RaidUserRoleRepository>()
                                          .GetQuery();

        _userRaidRoles = _repositoryFactory.GetRepository<DiscordServerMemberRepository>()
                                           .GetQuery()
                                           .Where(member => member.ServerId == WebAppConfiguration.DiscordServerId)
                                           .Select(member => new
                                                             {
                                                                 member.Name,
                                                                 Roles = raidRoles.Where(raidRole => raidRole.UserId == member.Account.UserId)
                                                                                  .Select(raidRole => raidRole.RoleId)
                                                                                  .ToList()
                                                             })
                                           .Where(member => member.Roles.Any())
                                           .AsEnumerable()
                                           .Select(member => new UserRaidRoleDTO
                                                             {
                                                                 Name = member.Name,
                                                                 Roles = member.Roles.Aggregate(RaidRole.None,
                                                                                                (current, role) => current
                                                                                                                   | role switch
                                                                                                                     {
                                                                                                                         1 => RaidRole.DamageDealer,
                                                                                                                         2 => RaidRole.AlacrityDamageDealer,
                                                                                                                         3 => RaidRole.QuicknessDamageDealer,
                                                                                                                         4 => RaidRole.AlacrityHealer,
                                                                                                                         5 => RaidRole.QuicknessHealer,
                                                                                                                         8 => RaidRole.AlacrityTankHealer,
                                                                                                                         9 => RaidRole.QuicknessTankHealer,
                                                                                                                         _ => current
                                                                                                                     })
                                                             })
                                           .ToList();
    }

    #endregion // ComponentBase
}
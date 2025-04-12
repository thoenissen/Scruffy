using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Authorization;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.WebApp.Components.Pages.Raid.Data;
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

    /// <summary>
    /// List of raid days
    /// </summary>
    private List<RaidDayStatisticsDTO> _days;

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Get count of participants
    /// </summary>
    /// <param name="users">Users</param>
    /// <param name="day">Day</param>
    /// <param name="filter">Filter</param>
    /// <returns>Count of participants</returns>
    private static double GetParticipantCount(List<UserRaidAppointmentsData> users, DayOfWeek day, Func<UserRaidAppointmentsData, bool> filter)
    {
        var points = users.Where(filter)
                          .Sum(user => user.Appointments.Where(obj => obj.DayOfWeek == day)
                                                        .Sum(obj2 => Math.Pow(10, -(((DateTime.Today - obj2).Days / 7) - 15) / 14.6)));

        return points / 66.147532745646117;
    }

    #endregion // Methods

    #region ComponentBase

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        var raidRoles = _repositoryFactory.GetRepository<RaidUserRoleRepository>()
                                          .GetQuery();

        var specialRoles = _repositoryFactory.GetRepository<RaidUserSpecialRoleRepository>()
                                             .GetQuery();

        _userRaidRoles = _repositoryFactory.GetRepository<DiscordServerMemberRepository>()
                                           .GetQuery()
                                           .Where(member => member.ServerId == WebAppConfiguration.DiscordServerId)
                                           .Select(member => new
                                                             {
                                                                 member.Name,
                                                                 Roles = raidRoles.Where(raidRole => raidRole.UserId == member.Account.UserId)
                                                                                  .Select(raidRole => raidRole.RoleId)
                                                                                  .ToList(),
                                                                 SpecialRoles = specialRoles.Where(specialRole => specialRole.UserId == member.Account.UserId)
                                                                                            .Select(specialRole => specialRole.SpecialRoleId)
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
                                                                                                                     }),
                                                                 SpecialRoles = member.SpecialRoles.Aggregate(RaidSpecialRole.None,
                                                                                                              (current, role) => current
                                                                                                                                 | role switch
                                                                                                                                   {
                                                                                                                                       1 => RaidSpecialRole.HandKiter,
                                                                                                                                       2 => RaidSpecialRole.SoullessHorrorPusher,
                                                                                                                                       3 => RaidSpecialRole.Quadim1Kiter,
                                                                                                                                       4 => RaidSpecialRole.Quadim2Kiter,
                                                                                                                                       _ => current
                                                                                                                                   })
                                                             })
                                           .ToList();

        _days = _repositoryFactory.GetRepository<RaidDayConfigurationRepository>()
                                             .GetQuery()
                                             .Select(day => new RaidDayStatisticsDTO
                                                            {
                                                                Day = day.Day,
                                                            })
                                             .ToList();

        var dateLimit = DateTime.Today.AddDays(-7 * 15);

        var users = _repositoryFactory.GetRepository<RaidRegistrationRepository>()
                     .GetQuery()
                     .Where(obj => obj.Points != null
                                && obj.RaidAppointment.TimeStamp > dateLimit)
                     .Select(obj => new
                                    {
                                        obj.UserId,
                                        obj.RaidAppointment.TimeStamp,
                                    })
                     .GroupBy(obj => obj.UserId)
                     .Select(obj => new
                                    {
                                        UserId = obj.Key,
                                        Appointments = obj.Select(obj2 => obj2.TimeStamp)
                                                          .ToList(),
                                        Roles = raidRoles.Where(raidRole => raidRole.UserId == obj.Key)
                                                         .Select(raidRole => raidRole.RoleId)
                                                         .ToList(),
                                        SpecialRoles = specialRoles.Where(specialRole => specialRole.UserId == obj.Key)
                                                                   .Select(specialRole => specialRole.SpecialRoleId)
                                                                   .ToList()
                                    })
                     .AsEnumerable()
                     .Select(user => new UserRaidAppointmentsData
                                     {
                                         Appointments = user.Appointments,
                                         Roles = user.Roles.Aggregate(RaidRole.None,
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
                                                                                         }),
                                         SpecialRoles = user.SpecialRoles.Aggregate(RaidSpecialRole.None,
                                                                                    (current, role) => current
                                                                                                       | role switch
                                                                                                       {
                                                                                                           1 => RaidSpecialRole.HandKiter,
                                                                                                           2 => RaidSpecialRole.SoullessHorrorPusher,
                                                                                                           3 => RaidSpecialRole.Quadim1Kiter,
                                                                                                           4 => RaidSpecialRole.Quadim2Kiter,
                                                                                                           _ => current
                                                                                                       })
                                     })
                     .ToList();

        foreach (var day in _days)
        {
            day.DamageDealer = GetParticipantCount(users, day.Day, user => user.Roles.HasFlag(RaidRole.DamageDealer));
            day.AlacrityDamageDealer = GetParticipantCount(users, day.Day, user => user.Roles.HasFlag(RaidRole.AlacrityDamageDealer));
            day.QuicknessDamageDealer = GetParticipantCount(users, day.Day, user => user.Roles.HasFlag(RaidRole.QuicknessDamageDealer));
            day.AlacrityHealer = GetParticipantCount(users, day.Day, user => user.Roles.HasFlag(RaidRole.AlacrityHealer));
            day.QuicknessHealer = GetParticipantCount(users, day.Day, user => user.Roles.HasFlag(RaidRole.QuicknessHealer));
            day.AlacrityTankHealer = GetParticipantCount(users, day.Day, user => user.Roles.HasFlag(RaidRole.AlacrityTankHealer));
            day.QuicknessTankHealer = GetParticipantCount(users, day.Day, user => user.Roles.HasFlag(RaidRole.QuicknessTankHealer));
            day.HandKiter = GetParticipantCount(users, day.Day, user => user.SpecialRoles.HasFlag(RaidSpecialRole.HandKiter));
            day.SoullessHorrorPusher = GetParticipantCount(users, day.Day, user => user.SpecialRoles.HasFlag(RaidSpecialRole.SoullessHorrorPusher));
            day.Quadim1Kiter = GetParticipantCount(users, day.Day, user => user.SpecialRoles.HasFlag(RaidSpecialRole.Quadim1Kiter));
            day.Quadim2Kiter = GetParticipantCount(users, day.Day, user => user.SpecialRoles.HasFlag(RaidSpecialRole.Quadim2Kiter));
        }
    }

    #endregion // ComponentBase
}
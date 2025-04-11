using System;
using System.Collections.Generic;
using System.Linq;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.WebApp.DTOs.Raid;

namespace Scruffy.WebApp.Components.Pages.Raid;

/// <summary>
/// Line up
/// </summary>
public partial class LineUp
{
    #region Fields

    /// <summary>
    /// Time stamp of the appointment
    /// </summary>
    private DateTime? _timeStamp;

    /// <summary>
    /// Number of groups
    /// </summary>
    private int _groupCount;

    /// <summary>
    /// Registrations
    /// </summary>
    private List<PlayerDTO> _registrations;

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Called when the squad assignment has changed
    /// </summary>
    private void OnSquadAssignmentChanged()
    {
        StateHasChanged();
    }

    #endregion // Methods

    #region ComponentBase

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var appointment = dbFactory.GetRepository<RaidAppointmentRepository>()
                                       .GetQuery()
                                       .Where(appointment => appointment.IsCommitted == false
                                                              && appointment.TimeStamp > DateTime.Now)
                                       .OrderBy(appointment => appointment.TimeStamp)
                                       .FirstOrDefault();

            _timeStamp = appointment?.TimeStamp;
            _groupCount = appointment?.GroupCount ?? 0;
            _groupCount = 2;

            _registrations = dbFactory.GetRepository<RaidRegistrationRepository>()
                                      .GetQuery()
                                      .Where(registration => registration.LineupExperienceLevelId != null
                                                             && registration.AppointmentId == appointment.Id)
                                      .Select(registration => new
                                                              {
                                                                  Id = registration.UserId,
                                                                  Name = registration.User.DiscordAccounts.Select(account => account.Members.Where(member => member.ServerId == WebAppConfiguration.DiscordServerId)
                                                                                                                                            .Select(member => member.Name)
                                                                                                                                            .FirstOrDefault())
                                                                                                          .FirstOrDefault(),
                                                                  Roles = registration.User.RaidUserRoles.Select(userRole => userRole.RoleId)
                                                                                                         .ToList(),
                                                              })
                                      .AsEnumerable()
                                      .Select(registration => new PlayerDTO
                                                              {
                                                                  Id = registration.Id,
                                                                  Name = registration.Name,
                                                                  Roles = registration.Roles.Aggregate(RaidRole.DamageDealer,
                                                                                                       (current, role) => current | role switch
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
                                                              })
                    .ToList();
        }
    }

    #endregion // ComponentBase
}
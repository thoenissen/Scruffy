using System.Data;

using Discord.Rest;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.ServiceHosts.WebApi.DTO.Raid;
using Scruffy.Services.Core.Extensions;
using Scruffy.Services.Raid;

namespace Scruffy.ServiceHosts.WebApi.Controllers;

/// <summary>
/// Raid controller
/// </summary>
[ApiController]
[Route("raid")]
#if !DEBUG
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administrator")]
#endif
public class RaidController : ControllerBase
{
    #region Fields

    /// <summary>
    /// Discord client
    /// </summary>
    private readonly DiscordRestClient _discordClient;

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory;

    /// <summary>
    /// Raid role service
    /// </summary>
    private readonly RaidRolesService _raidRolesService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="discordClient">Discord client</param>
    /// <param name="repositoryFactory">Repository factory</param>
    /// <param name="raidRolesService">Raid roles service</param>
    public RaidController(DiscordRestClient discordClient,
                          RepositoryFactory repositoryFactory,
                          RaidRolesService raidRolesService)
    {
        _discordClient = discordClient;
        _repositoryFactory = repositoryFactory;
        _raidRolesService = raidRolesService;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Get a list of all active appointments
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet]
    [Route("appointments")]
    [Produces(typeof(List<ActiveRaidAppointmentDTO>))]
    public async Task<IActionResult> GetAppointments()
    {
        var discordAccounts = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                                .GetQuery()
                                                .Select(obj => obj);

        var userRoles = _repositoryFactory.GetRepository<RaidUserRoleRepository>()
                                          .GetQuery()
                                          .Select(obj => obj);

        var registrationRoles = _repositoryFactory.GetRepository<RaidRegistrationRoleAssignmentRepository>()
                                                  .GetQuery()
                                                  .Select(obj => obj);

        var appointments = new List<ActiveRaidAppointmentDTO>();

        foreach (var appointment in _repositoryFactory.GetRepository<RaidAppointmentRepository>()
                                                      .GetQuery()
                                                      .Where(obj => obj.IsCommitted == false)
                                                      .Select(obj => new
                                                                     {
                                                                         obj.Id,
                                                                         obj.TimeStamp,
                                                                         obj.RaidDayTemplate.Title,
                                                                         Participants = obj.RaidRegistrations
                                                                                           .Where(obj2 => obj2.LineupExperienceLevelId != null)
                                                                                           .Select(obj2 => new
                                                                                                           {
                                                                                                               Id = obj2.UserId,
                                                                                                               DiscordAccountId = discordAccounts.Where(obj3 => obj3.UserId == obj2.UserId)
                                                                                                                                                 .Select(obj3 => (ulong?)obj3.Id)
                                                                                                                                                 .FirstOrDefault(),
                                                                                                               Roles = userRoles.Where(obj3 => obj3.UserId == obj2.UserId)
                                                                                                                                .Select(obj3 => obj3.RoleId)
                                                                                                                                .ToList(),
                                                                                                               PreferredRoles = registrationRoles.Where(obj3 => obj3.RegistrationId == obj2.Id)
                                                                                                                                                 .Select(obj3 => obj3.RoleId)
                                                                                                                                                 .ToList()
                                                                                                           })
                                                                                           .ToList()
                                                                     }))
        {
            var participants = new List<RaidParticipantDTO>();

            foreach (var participant in appointment.Participants)
            {
                string name = null;

                if (participant.DiscordAccountId != null)
                {
                    var member = await _discordClient.GetGuildUserAsync(WebApiConfiguration.DiscordServerId, participant.DiscordAccountId.Value)
                                                     .ConfigureAwait(false);

                    name = member.TryGetDisplayName();
                }

                participants.Add(new RaidParticipantDTO
                                 {
                                     Id = participant.Id,
                                     Name = name,
                                     Roles = participant.Roles
                                                        .Concat(participant.PreferredRoles)
                                                        .Distinct()
                                                        .ToList(),
                                     PreferredRoles = participant.PreferredRoles
                                 });
            }

            appointments.Add(new ActiveRaidAppointmentDTO
                             {
                                 Id = appointment.Id,
                                 TimeStamp = appointment.TimeStamp,
                                 Title = appointment.Title,
                                 Participants = participants
                             });
        }

        return Ok(appointments);
    }

    /// <summary>
    /// Get a list of all roles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet]
    [Route("roles")]
    [Produces(typeof(List<RaidRoleDTO>))]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _repositoryFactory.GetRepository<RaidRoleRepository>()
                                            .GetQuery()
                                            .Select(obj => obj)
                                            .ToListAsync()
                                            .ConfigureAwait(false);

        return Ok(roles.Select(obj => new RaidRoleDTO
                                      {
                                          Id = obj.Id,
                                          Description = _raidRolesService.GetDescriptionAsText(obj)
                                      })
                       .ToList());
    }

    /// <summary>
    /// Get all raid relevant users
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet]
    [Route("users")]
    [Produces(typeof(List<RaidUserDTO>))]
    public async Task<IActionResult> GetUsers()
    {
        var discordAccounts = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                                .GetQuery()
                                                .Select(obj => obj);

        var users = new List<RaidUserDTO>();

        foreach (var user in await _repositoryFactory.GetRepository<RaidUserRoleRepository>()
                                                     .GetQuery()
                                                     .GroupBy(obj => obj.UserId)
                                                     .Select(obj => new
                                                                    {
                                                                        Id = obj.Key,
                                                                        DiscordAccountId = discordAccounts.Where(obj2 => obj2.UserId == obj.Key)
                                                                                                          .Select(obj2 => (ulong?)obj2.Id)
                                                                                                          .FirstOrDefault(),
                                                                        AssignedRoles = obj.Select(obj2 => obj2.RoleId)
                                                                                           .ToList()
                                                                    })
                                                     .ToListAsync()
                                                     .ConfigureAwait(false))
        {
            string name = null;

            if (user.DiscordAccountId != null)
            {
                var member = await _discordClient.GetGuildUserAsync(WebApiConfiguration.DiscordServerId, user.DiscordAccountId.Value)
                                                 .ConfigureAwait(false);

                name = member.TryGetDisplayName();
            }

            users.Add(new RaidUserDTO
                      {
                          Id = user.Id,
                          Name = name,
                          AssignedRoles = user.AssignedRoles
                      });
        }

        return Ok(users);
    }

    /// <summary>
    /// Post the line up of the given appointment
    /// </summary>
    /// <param name="lineUp">Line up</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost]
    [Route("lineUp")]
    public async Task<IActionResult> PostLineUp(LineUpDTO lineUp)
    {
        var success = true;

        var transaction = _repositoryFactory.BeginTransaction(IsolationLevel.RepeatableRead);
        await using (transaction.ConfigureAwait(false))
        {
            foreach (var group in lineUp.Groups)
            {
                foreach (var user in group.Value)
                {
                    if (_repositoryFactory.GetRepository<RaidRegistrationRepository>()
                                      .Refresh(obj => obj.AppointmentId == lineUp.AppointmentId
                                                      && obj.UserId == user.UserId,
                                               obj =>
                                               {
                                                   obj.Group = group.Key;
                                                   obj.LineUpRoleId = user.RoleId;
                                               }) == false)
                    {
                        success = false;
                        break;
                    }
                }

                if (success == false)
                {
                    break;
                }
            }

            if (success)
            {
                await transaction.CommitAsync()
                                 .ConfigureAwait(false);
            }
        }

        return success ? Ok() : BadRequest();
    }

    #endregion // Methods
}
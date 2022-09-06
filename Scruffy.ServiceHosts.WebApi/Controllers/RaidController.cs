using System.Data;

using Discord.Rest;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
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

    /// <summary>
    /// Raid line up service
    /// </summary>
    private readonly RaidLineUpService _raidLineUpService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="discordClient">Discord client</param>
    /// <param name="repositoryFactory">Repository factory</param>
    /// <param name="raidRolesService">Raid roles service</param>
    /// <param name="raidLineUpService">Raid line up service</param>
    public RaidController(DiscordRestClient discordClient,
                          RepositoryFactory repositoryFactory,
                          RaidRolesService raidRolesService,
                          RaidLineUpService raidLineUpService)
    {
        _discordClient = discordClient;
        _repositoryFactory = repositoryFactory;
        _raidRolesService = raidRolesService;
        _raidLineUpService = raidLineUpService;
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

        var discordMembers = _repositoryFactory.GetRepository<DiscordServerMemberRepository>()
                                               .GetQuery()
                                               .Select(obj => obj);

        var userRoles = _repositoryFactory.GetRepository<RaidUserRoleRepository>()
                                          .GetQuery()
                                          .Select(obj => obj);

        var registrationRoles = _repositoryFactory.GetRepository<RaidRegistrationRoleAssignmentRepository>()
                                                  .GetQuery()
                                                  .Select(obj => obj);

        var appointments = await _repositoryFactory.GetRepository<RaidAppointmentRepository>()
                                                   .GetQuery()
                                                   .Where(obj => obj.IsCommitted == false)
                                                   .Select(obj => new ActiveRaidAppointmentDTO
                                                                  {
                                                                      Id = obj.Id,
                                                                      TimeStamp = obj.TimeStamp,
                                                                      Title = obj.RaidDayTemplate.Title,
                                                                      Participants = obj.RaidRegistrations
                                                                                                       .Where(obj2 => obj2.LineupExperienceLevelId != null)
                                                                                                       .Select(obj2 => new RaidParticipantDTO
                                                                                                       {
                                                                                                           Id = obj2.UserId,
                                                                                                           Name = discordMembers.Where(obj3 => obj3.ServerId == WebApiConfiguration.DiscordServerId
                                                                                                                                            && obj3.AccountId == discordAccounts.Where(obj4 => obj4.UserId == obj2.UserId)
                                                                                                                                                                                .Select(obj4 => (ulong?)obj4.Id)
                                                                                                                                                                                .FirstOrDefault())
                                                                                                                                                .Select(obj3 => obj3.Name)
                                                                                                                                                .FirstOrDefault()
                                                                                                                              ?? obj2.User.Name,
                                                                                                           Roles = userRoles.Where(obj3 => obj3.UserId == obj2.UserId)
                                                                                                                                            .Select(obj3 => obj3.RoleId)
                                                                                                                                            .ToList(),
                                                                                                           PreferredRoles = registrationRoles.Where(obj3 => obj3.RegistrationId == obj2.Id)
                                                                                                                                                             .Select(obj3 => obj3.RoleId)
                                                                                                                                                             .ToList()
                                                                                                       })
                                                                                                       .ToList()
                                                                  })
                                                   .ToListAsync()
                                                   .ConfigureAwait(false);

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

        var discordMembers = _repositoryFactory.GetRepository<DiscordServerMemberRepository>()
                                               .GetQuery()
                                               .Select(obj => obj);

        var users = _repositoryFactory.GetRepository<UserRepository>()
                                      .GetQuery()
                                      .Select(obj => obj);

        var raidUsers = await _repositoryFactory.GetRepository<RaidUserRoleRepository>()
                                                .GetQuery()
                                                .GroupBy(obj => obj.UserId)
                                                .Select(obj => new RaidUserDTO
                                                               {
                                                                   Id = obj.Key,
                                                                   Name = discordMembers.Where(obj2 => obj2.ServerId == WebApiConfiguration.DiscordServerId
                                                                                                    && obj2.AccountId == discordAccounts.Where(obj3 => obj3.UserId == obj.Key)
                                                                                                                                        .Select(obj3 => (ulong?)obj3.Id)
                                                                                                                                        .FirstOrDefault())
                                                                                        .Select(obj2 => obj2.Name)
                                                                                        .FirstOrDefault()
                                                                       ?? users.Where(obj2 => obj2.Id == obj.Key)
                                                                               .Select(obj2 => obj2.Name)
                                                                               .FirstOrDefault(),
                                                                   AssignedRoles = obj.Select(obj2 => obj2.RoleId)
                                                                                      .ToList()
                                                               })
                                                .ToListAsync()
                                                .ConfigureAwait(false);

        return Ok(raidUsers);
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
            _repositoryFactory.GetRepository<RaidRegistrationRepository>()
                              .RefreshRange(obj => obj.AppointmentId == lineUp.AppointmentId,
                                            obj =>
                                            {
                                                obj.Group = 0;
                                                obj.LineUpRoleId = null;
                                            });

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

        if (success)
        {
            await _raidLineUpService.PostLineUp(lineUp.AppointmentId)
                                    .ConfigureAwait(false);
        }

        return success ? Ok() : BadRequest();
    }

    #endregion // Methods
}
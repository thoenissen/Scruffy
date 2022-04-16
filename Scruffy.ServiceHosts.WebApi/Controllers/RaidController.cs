using Discord.Rest;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.ServiceHosts.WebApi.DTO.Raid;
using Scruffy.Services.Core.Extensions;

namespace Scruffy.ServiceHosts.WebApi.Controllers;

/// <summary>
/// Raid controller
/// </summary>
[ApiController]
[Route("[controller]")]
#if !DEBUG
[Authorize]
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

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="discordClient">Discord client</param>
    /// <param name="repositoryFactory">Repository factory</param>
    public RaidController(DiscordRestClient discordClient, RepositoryFactory repositoryFactory)
    {
        _discordClient = discordClient;
        _repositoryFactory = repositoryFactory;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Get a list of all active appointments
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet]
    [Route("Appointments")]
    [Produces(typeof(List<ActiveRaidAppointmentDTO>))]
    public async Task<IActionResult> GetAppointments()
    {
        var discordAccounts = _repositoryFactory.GetRepository<DiscordAccountRepository>()
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
                                                                                                                                                 .FirstOrDefault()
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

    #endregion // Methods
}
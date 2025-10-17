using System.Data;
using System.Net.Http;

using Discord;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Services.Raid;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Interfaces;
using Scruffy.Services.Raid.DialogElements;

namespace Scruffy.Services.Raid;

/// <summary>
/// Managing raid registrations
/// </summary>
public class RaidRegistrationService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// User management
    /// </summary>
    private readonly UserManagementService _userManagementService;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="userManagementService">User management</param>
    /// <param name="httpClientFactory">HttpClient-Factory</param>
    public RaidRegistrationService(LocalizationService localizationService, UserManagementService userManagementService, IHttpClientFactory httpClientFactory)
        : base(localizationService)
    {
        _userManagementService = userManagementService;
    }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Joining a appointment
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <param name="appointmentId">Id of the appointment</param>
    /// <param name="discordUser">User</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<long?> Join(IContextContainer commandContext, long appointmentId, IUser discordUser)
    {
        long? registrationId = null;

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var isAlreadyRegistered = false;

            var user = await _userManagementService.GetUserByDiscordAccountId(discordUser)
                                                   .ConfigureAwait(false);

            if (dbFactory.GetRepository<RaidAppointmentRepository>()
                         .GetQuery()
                         .Any(obj => obj.Id == appointmentId
                                  && obj.RaidDayTemplate.RaidExperienceAssignments.Any(obj2 => obj2.RaidExperienceLevel.Rank >= user.ExperienceLevelRank)))
            {
                var success = true;

                var transaction = dbFactory.BeginTransaction(IsolationLevel.RepeatableRead);

                await using (transaction.ConfigureAwait(false))
                {
                    if (dbFactory.GetRepository<RaidRegistrationRepository>()
                                 .AddOrRefresh(obj => obj.AppointmentId == appointmentId
                                                   && obj.UserId == user.Id,
                                               obj =>
                                               {
                                                   if (obj.Id == 0)
                                                   {
                                                       obj.AppointmentId = appointmentId;
                                                       obj.UserId = user.Id;
                                                       obj.RegistrationTimeStamp = DateTime.Now;
                                                   }
                                                   else
                                                   {
                                                       isAlreadyRegistered = true;
                                                   }
                                               },
                                               obj => registrationId = obj.Id))
                    {
                        if (registrationId != null
                         && isAlreadyRegistered == false)
                        {
                            var registration = dbFactory.GetRepository<RaidRegistrationRepository>()
                                                        .GetQuery()
                                                        .Where(obj => obj.Id == registrationId)
                                                        .Select(obj => new
                                                                       {
                                                                           IsDeadlineReached = obj.RaidAppointment.Deadline < obj.RegistrationTimeStamp,
                                                                           Registrations = obj.RaidAppointment
                                                                                              .RaidRegistrations
                                                                                              .Count(obj2 => obj2.LineupExperienceLevelId == obj.User.RaidExperienceLevelId),
                                                                           AvailableSlots = obj.RaidAppointment
                                                                                               .RaidDayTemplate
                                                                                               .RaidExperienceAssignments
                                                                                               .Where(obj2 => obj2.ExperienceLevelId == obj.User.RaidExperienceLevelId)
                                                                                               .Select(obj2 => (int?)obj2.Count)
                                                                                               .FirstOrDefault(),
                                                                           obj.User.RaidExperienceLevelId
                                                                       })
                                                        .FirstOrDefault();

                            if (registration != null)
                            {
                                if (registration.IsDeadlineReached == false)
                                {
                                    if (registration.AvailableSlots != null
                                        && registration.AvailableSlots > registration.Registrations)
                                    {
                                        success = dbFactory.GetRepository<RaidRegistrationRepository>()
                                                           .Refresh(obj => obj.Id == registrationId,
                                                                    obj => obj.LineupExperienceLevelId = registration.RaidExperienceLevelId);
                                    }
                                    else
                                    {
                                        success = await RefreshAppointment(dbFactory, appointmentId).ConfigureAwait(false);
                                    }
                                }
                            }
                            else
                            {
                                success = false;
                            }
                        }
                    }
                    else
                    {
                        success = false;
                    }

                    if (success)
                    {
                        await transaction.CommitAsync()
                                         .ConfigureAwait(false);
                    }
                    else
                    {
                        registrationId = null;
                    }
                }
            }
            else
            {
                await commandContext.ReplyAsync(LocalizationGroup.GetText("RequiredExperienceLevelMissing", "You don't have the required experience level."), ephemeral: true)
                                    .ConfigureAwait(false);
            }
        }

        return registrationId;
    }

    /// <summary>
    /// Leaving a appointment
    /// </summary>
    /// <param name="appointmentId">Id of the appointment</param>
    /// <param name="userId">User id</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<bool> Leave(long appointmentId, long userId)
    {
        var success = false;

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var transaction = dbFactory.BeginTransaction(IsolationLevel.RepeatableRead);

            await using (transaction.ConfigureAwait(false))
            {
                var now = DateTime.Now;

                var registration = dbFactory.GetRepository<RaidRegistrationRepository>()
                                            .GetQuery()
                                            .Where(obj => obj.AppointmentId == appointmentId
                                                       && obj.UserId == userId)
                                            .Select(obj => new
                                                           {
                                                               obj.Id,
                                                               IsDeadlineReached = obj.RaidAppointment.Deadline < now,
                                                               Registrations = obj.RaidAppointment
                                                                                  .RaidRegistrations
                                                                                  .Count(obj2 => obj2.LineupExperienceLevelId == obj.User.RaidExperienceLevelId),
                                                               AvailableSlots = obj.RaidAppointment
                                                                                   .RaidDayTemplate
                                                                                   .RaidExperienceAssignments
                                                                                   .Where(obj2 => obj2.ExperienceLevelId == obj.User.RaidExperienceLevelId)
                                                                                   .Select(obj2 => (int?)obj2.Count)
                                                                                   .FirstOrDefault(),
                                                               obj.User.RaidExperienceLevelId
                                                           })
                                            .FirstOrDefault();

                if (registration != null)
                {
                    dbFactory.GetRepository<RaidRegistrationRoleAssignmentRepository>()
                             .RemoveRange(obj => obj.RegistrationId == registration.Id);

                    dbFactory.GetRepository<RaidRegistrationRepository>()
                             .Remove(obj => obj.Id == registration.Id);

                    if ((registration.AvailableSlots != null
                         && registration.AvailableSlots > registration.Registrations)
                        || registration.IsDeadlineReached)
                    {
                        success = true;
                    }
                    else
                    {
                        success = await RefreshAppointment(dbFactory, appointmentId).ConfigureAwait(false);
                    }
                }
                else
                {
                    success = true;
                }

                if (success)
                {
                    await transaction.CommitAsync()
                                     .ConfigureAwait(false);
                }
            }
        }

        return success;
    }

    /// <summary>
    /// Set template
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <param name="appointmentId">Id of the appointment</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<bool> SetTemplate(IContextContainer commandContext, long appointmentId)
    {
        var success = false;

        var dialogHandler = new DialogHandler(commandContext);

        await using (dialogHandler.ConfigureAwait(false))
        {
            var templateId = await dialogHandler.Run<RaidTemplateSelectionDialogElement, long>()
                                                .ConfigureAwait(false);

            if (templateId > 0)
            {
                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    var transaction = dbFactory.BeginTransaction(IsolationLevel.RepeatableRead);

                    await using (transaction.ConfigureAwait(false))
                    {
                        success = dbFactory.GetRepository<RaidAppointmentRepository>()
                                           .Refresh(obj => obj.Id == appointmentId,
                                                    obj => obj.TemplateId = templateId);

                        if (success)
                        {
                            await RefreshAppointment(dbFactory, appointmentId).ConfigureAwait(false);
                        }

                        if (success)
                        {
                            await transaction.CommitAsync()
                                             .ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        return success;
    }

    /// <summary>
    /// Set group count
    /// </summary>
    /// <param name="appointmentId">Id of the appointment</param>
    /// <param name="groupCount">Group count</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<bool> SetGroupCount(long appointmentId, int groupCount)
    {
        var success = false;

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var transaction = dbFactory.BeginTransaction(IsolationLevel.RepeatableRead);

            await using (transaction.ConfigureAwait(false))
            {
                success = dbFactory.GetRepository<RaidAppointmentRepository>()
                                   .Refresh(obj => obj.Id == appointmentId,
                                            obj => obj.GroupCount = groupCount);

                if (success)
                {
                    success = await RefreshAppointment(dbFactory, appointmentId).ConfigureAwait(false);
                }

                if (success)
                {
                    await transaction.CommitAsync()
                                     .ConfigureAwait(false);
                }
            }
        }

        return success;
    }

    /// <summary>
    /// Refreshing the appointment
    /// </summary>
    /// <param name="dbFactory">Repository factory</param>
    /// <param name="appointmentId">Id of the appointment</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<bool> RefreshAppointment(RepositoryFactory dbFactory, long appointmentId)
    {
        var success = false;

        var defaultRank = dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                   .GetQuery()
                                   .Max(obj => obj.Rank);

        var currentRaidPoints = dbFactory.GetRepository<RaidCurrentUserPointsRepository>()
                                         .GetQuery()
                                         .Select(obj => obj);

        var appointment = await dbFactory.GetRepository<RaidAppointmentRepository>()
                                         .GetQuery()
                                         .Where(obj => obj.Id == appointmentId)
                                         .Select(obj => new
                                                        {
                                                            obj.GroupCount,
                                                            ExperienceLevels = obj.RaidDayTemplate
                                                                                  .RaidExperienceAssignments
                                                                                  .Select(obj2 => new
                                                                                                  {
                                                                                                      obj2.ExperienceLevelId,
                                                                                                      obj2.RaidExperienceLevel.Rank,
                                                                                                      obj2.Count
                                                                                                  })
                                                                                  .ToList(),
                                                            Registrations = obj.RaidRegistrations
                                                                               .OrderByDescending(obj2 => currentRaidPoints.Where(obj3 => obj3.UserId == obj2.UserId)
                                                                                                                           .Select(obj3 => obj3.Points)
                                                                                                                           .FirstOrDefault())
                                                                               .ThenBy(obj2 => obj2.RegistrationTimeStamp)
                                                                               .Select(obj2 => new
                                                                                               {
                                                                                                   obj2.Id,
                                                                                                   Rank = (int?)obj2.User.RaidExperienceLevel.Rank,
                                                                                                   IsBevorDeadline = obj2.RegistrationTimeStamp < obj.Deadline
                                                                                               })
                                                                               .ToList()
                                                        })
                                         .FirstOrDefaultAsync()
                                         .ConfigureAwait(false);

        if (appointment != null)
        {
            var slots = appointment.ExperienceLevels
                                   .OrderBy(obj => obj.Rank)
                                   .Select(obj => new RaidAppointmentSlotData
                                                  {
                                                      ExperienceLevelId = obj.ExperienceLevelId,
                                                      Registrations = [],
                                                      Rank = obj.Rank,
                                                      SlotCount = obj.Count * appointment.GroupCount
                                                  })
                                   .ToList();

            // Assigning the users to the slots
            var remainingRegistrations = new List<long>();

            foreach (var registration in appointment.Registrations.Where(obj => obj.IsBevorDeadline))
            {
                var slot = slots.FirstOrDefault(obj => obj.SlotCount > obj.Registrations.Count
                                                    && obj.Rank >= (registration.Rank ?? defaultRank));

                if (slot != null)
                {
                    slot.Registrations.Add(registration.Id);
                }
                else
                {
                    remainingRegistrations.Add(registration.Id);
                }
            }

            var substitutesBench = appointment.Registrations.Where(obj => obj.IsBevorDeadline == false).Select(obj => obj.Id).ToList();

            foreach (var registrationId in remainingRegistrations)
            {
                var slot = slots.FirstOrDefault(obj => obj.SlotCount > obj.Registrations.Count);

                if (slot != null)
                {
                    slot.Registrations.Add(registrationId);
                }
                else
                {
                    substitutesBench.Add(registrationId);
                }
            }

            success = true;

            // Updating the database entries
            foreach (var slot in slots)
            {
                if (dbFactory.GetRepository<RaidRegistrationRepository>()
                             .RefreshRange(obj => slot.Registrations.Contains(obj.Id),
                                           obj => obj.LineupExperienceLevelId = slot.ExperienceLevelId) == false)
                {
                    success = false;

                    break;
                }
            }

            if (success
             && substitutesBench.Count > 0)
            {
                success = dbFactory.GetRepository<RaidRegistrationRepository>()
                                   .RefreshRange(obj => substitutesBench.Contains(obj.Id),
                                                 obj => obj.LineupExperienceLevelId = null);
            }
        }

        return success;
    }

    #endregion // Methods
}
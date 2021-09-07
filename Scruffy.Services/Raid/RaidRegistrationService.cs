using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Services.Raid;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Raid.DialogElements;

namespace Scruffy.Services.Raid
{
    /// <summary>
    /// Managing raid registrations
    /// </summary>
    public class RaidRegistrationService : LocatedServiceBase
    {
        #region Properties

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public RaidRegistrationService(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Joining a appointment
        /// </summary>
        /// <param name="appointmentId">Id of the appointment</param>
        /// <param name="userId">User id</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<long?> Join(long appointmentId, ulong userId)
        {
            long? registrationId = null;

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var isAlreadyRegistered = false;

                if (dbFactory.GetRepository<RaidRegistrationRepository>()
                             .AddOrRefresh(obj => obj.AppointmentId == appointmentId && obj.UserId == userId,
                                           obj =>
                                           {
                                               if (obj.Id == 0)
                                               {
                                                   obj.AppointmentId = appointmentId;
                                                   obj.UserId = userId;
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

                        if (registration?.IsDeadlineReached == false)
                        {
                            if (registration.AvailableSlots != null
                             && registration.AvailableSlots > registration.Registrations)
                            {
                                dbFactory.GetRepository<RaidRegistrationRepository>()
                                         .Refresh(obj => obj.Id == registrationId,
                                                  obj => obj.LineupExperienceLevelId = registration.RaidExperienceLevelId);
                            }
                            else
                            {
                                await RefreshAppointment(appointmentId).ConfigureAwait(false);
                            }
                        }
                    }
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
        public async Task<bool> Leave(long appointmentId, ulong userId)
        {
            var success = false;

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var registration = dbFactory.GetRepository<RaidRegistrationRepository>()
                                            .GetQuery()
                                            .Where(obj => obj.AppointmentId == appointmentId
                                                       && obj.UserId == userId)
                                            .Select(obj => new
                                            {
                                                obj.Id,
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
                    dbFactory.GetRepository<RaidRegistrationRoleAssignmentRepository>()
                             .RemoveRange(obj => obj.RegistrationId == registration.Id);

                    dbFactory.GetRepository<RaidRegistrationRepository>()
                             .Remove(obj => obj.Id == registration.Id);

                    if ((registration.AvailableSlots != null
                      && registration.AvailableSlots != registration.Registrations)
                     || registration.IsDeadlineReached)
                    {
                        success = true;
                    }
                    else
                    {
                        success = await RefreshAppointment(appointmentId).ConfigureAwait(false);
                    }
                }
                else
                {
                    success = true;
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
        public async Task<bool> SetTemplate(CommandContextContainer commandContext, long appointmentId)
        {
            var success = false;

            await using (var dialogHandler = new DialogHandler(commandContext))
            {
                var templateId = await dialogHandler.Run<RaidTemplateSelectionDialogElement, long>()
                                                    .ConfigureAwait(false);
                if (templateId > 0)
                {
                    using (var dbFactory = RepositoryFactory.CreateInstance())
                    {
                        success = dbFactory.GetRepository<RaidAppointmentRepository>()
                                           .Refresh(obj => obj.Id == appointmentId,
                                                    obj => obj.TemplateId = templateId);

                        if (success)
                        {
                            await RefreshAppointment(appointmentId).ConfigureAwait(false);
                        }
                    }
                }

                await dialogHandler.DeleteMessages()
                                   .ConfigureAwait(false);
            }

            return success;
        }

        /// <summary>
        /// Refreshing the appointment
        /// </summary>
        /// <param name="appointmentId">Id of the appointment</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<bool> RefreshAppointment(long appointmentId)
        {
            var success = false;

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                await using (var transaction = dbFactory.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
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
                                                                            .Select(obj2 => new
                                                                            {
                                                                                obj2.Id,
                                                                                Rank = (int?)obj2.User.RaidExperienceLevel.Rank
                                                                            })
                                                                            .ToList()
                                                     })
                                                     .FirstOrDefaultAsync()
                                                     .ConfigureAwait(false);

                    if (appointment != null)
                    {
                        var slotCountFactor = appointment.Registrations.Count / appointment.ExperienceLevels.Sum(obj => (double)obj.Count) > 1.4
                                                  ? 2
                                                  : 1;

                        var slots = appointment.ExperienceLevels
                                               .OrderBy(obj => obj.Rank)
                                               .Select(obj => new RaidAppointmentSlotData
                                               {
                                                   ExperienceLevelId = obj.ExperienceLevelId,
                                                   Registrations = new List<long>(),
                                                   Rank = obj.Rank,
                                                   SlotCount = obj.Count * slotCountFactor
                                               })
                                               .ToList();

                        // Assigning the users to the slots
                        var substitutesBench = new List<long>();

                        foreach (var registration in appointment.Registrations)
                        {
                            var slot = slots.FirstOrDefault(obj => obj.SlotCount > obj.Registrations.Count
                                                                && obj.Rank >= (registration.Rank ?? defaultRank));

                            if (slot != null)
                            {
                                slot.Registrations.Add(registration.Id);
                            }
                            else
                            {
                                substitutesBench.Add(registration.Id);
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

                        if (success)
                        {
                            await transaction.CommitAsync()
                                             .ConfigureAwait(false);
                        }
                    }
                }
            }

            return success;
        }

        #endregion // Methods
    }
}

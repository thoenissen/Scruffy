using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Fractals;
using Scruffy.Data.Entity.Tables.Fractals;
using Scruffy.Data.Services.Fractal;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Fractals.Jobs
{
    /// <summary>
    /// Fractal reminder creation
    /// </summary>
    public class FractalReminderJob : LocatedAsyncJob
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="timeStamp">Time stamp</param>
        public FractalReminderJob(DateTime timeStamp)
        {
            TimeStamp = timeStamp;
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Time stamp
        /// </summary>
        public DateTime TimeStamp { get; }

        #endregion // Properties

        #region LocatedAsyncJob

        /// <summary>
        /// Executes the job
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task ExecuteAsync()
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                FractalLfgMessageBuilder builder = null;

                var configurations = await dbFactory.GetRepository<FractalLfgConfigurationRepository>()
                                                    .GetQuery()
                                                    .Select(obj => new
                                                    {
                                                        ConfigurationId = obj.Id,
                                                        ChannelId = obj.DiscordChannelId,
                                                        Registrations = obj.FractalRegistrations
                                                                           .Select(obj2 => new AppointmentCreationRegistrationData
                                                                           {
                                                                               AppointmentTimeStamp = obj2.AppointmentTimeStamp,
                                                                               UserId = obj2.UserId,
                                                                               DiscordAccountId = obj2.User
                                                                                                      .DiscordAccounts
                                                                                                      .Select(obj3 => obj3.Id)
                                                                                                      .FirstOrDefault(),
                                                                               RegistrationTimeStamp = obj2.RegistrationTimeStamp
                                                                           })
                                                                           .Where(obj2 => obj2.AppointmentTimeStamp == TimeStamp)
                                                    })
                                                    .Where(obj => obj.Registrations.Count() >= 5)
                                                    .ToListAsync()
                                                    .ConfigureAwait(false);

                var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider();
                await using (serviceProvider.ConfigureAwait(false))
                {
                    var reminderService = serviceProvider.GetService<FractalLfgReminderService>();

                    foreach (var configuration in configurations)
                    {
                        var registrationsStack = new Stack<AppointmentCreationRegistrationData>(configuration.Registrations.OrderByDescending(obj => obj.RegistrationTimeStamp));

                        while (registrationsStack.Count >= 5)
                        {
                            var registrations = new List<AppointmentCreationRegistrationData>();

                            for (var i = 0; i < 5; i++)
                            {
                                registrations.Add(registrationsStack.Pop());
                            }

                            builder ??= serviceProvider.GetService<FractalLfgMessageBuilder>();

                            var messageId = await builder.CreateAppointmentMessage(configuration.ChannelId, TimeStamp, registrations)
                                                         .ConfigureAwait(false);

                            var appointment = new FractalAppointmentEntity
                                              {
                                                  ConfigurationId = configuration.ConfigurationId,
                                                  DiscordMessageId = messageId,
                                                  AppointmentTimeStamp = TimeStamp
                                              };

                            if (dbFactory.GetRepository<FractalAppointmentRepository>()
                                         .Add(appointment))
                            {
                                foreach (var registration in registrations)
                                {
                                    dbFactory.GetRepository<FractalRegistrationRepository>()
                                             .Refresh(obj => obj.ConfigurationId == configuration.ConfigurationId
                                                          && obj.AppointmentTimeStamp == TimeStamp
                                                          && obj.UserId == registration.UserId,
                                                      obj => obj.AppointmentId = appointment.Id);
                                }
                            }

                            reminderService.CreateReminderDeletionJob(configuration.ChannelId, messageId);
                        }
                    }

                    await reminderService.CreateNextReminderJobAsync()
                                         .ConfigureAwait(false);
                }
            }
        }

        #endregion // LocatedAsyncJob
    }
}

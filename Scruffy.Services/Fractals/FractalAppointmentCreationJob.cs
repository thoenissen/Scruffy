using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Fractals;
using Scruffy.Data.Entity.Tables.Fractals;
using Scruffy.Data.Services.Fractal;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Fractals
{
    /// <summary>
    /// Daily creation of the fractal appointments
    /// </summary>
    public class FractalAppointmentCreationJob : LocatedAsyncJob
    {
        #region AsyncJob

        /// <summary>
        /// Executes the job
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected override async Task ExecuteAsync()
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var configurations = await dbFactory.GetRepository<FractalLfgConfigurationRepository>()
                                                    .GetQuery()
                                                    .Select(obj => new
                                                                   {
                                                                       obj.Id
                                                                   })
                                                    .ToListAsync()
                                                    .ConfigureAwait(false);

                await using (var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider())
                {
                    var builder = serviceProvider.GetService<FractalLfgMessageBuilder>();

                    foreach (var configuration in configurations)
                    {
                        // Refreshing of the lfg message
                        await builder.RefreshMessageAsync(configuration.Id).ConfigureAwait(false);

                        // Deletion of old appointments
                        var date = DateTime.Today.AddDays(-1);

                        var discordClient = serviceProvider.GetService<DiscordClient>();

                        foreach (var entry in dbFactory.GetRepository<FractalAppointmentRepository>()
                                                       .GetQuery()
                                                       .Where(obj => obj.AppointmentTimeStamp.Date == date)
                                                       .Select(obj => new
                                                                      {
                                                                          obj.MessageId,
                                                                          obj.FractalLfgConfiguration.ChannelId
                                                                      })
                                                       .ToList())
                        {
                            try
                            {
                                var channel = await discordClient.GetChannelAsync(entry.ChannelId)
                                                                 .ConfigureAwait(false);
                                if (channel != null)
                                {
                                    var message = await channel.GetMessageAsync(entry.MessageId)
                                                               .ConfigureAwait(false);
                                    if (message != null)
                                    {
                                        await channel.DeleteMessageAsync(message)
                                                     .ConfigureAwait(false);
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }

                        // Creation of the appointments
                        date = DateTime.Today;

                        var channels = await dbFactory.GetRepository<FractalLfgConfigurationRepository>()
                                                      .GetQuery()
                                                      .Select(obj => new
                                                                     {
                                                                         ConfigurationId = obj.Id,
                                                                         obj.ChannelId,
                                                                         Registrations = obj.FractalRegistrations
                                                                                            .Select(obj2 => new AppointmentCreationRegistrationData
                                                                                                            {
                                                                                                                AppointmentTimeStamp = obj2.AppointmentTimeStamp,
                                                                                                                UserId = obj2.UserId,
                                                                                                                RegistrationTimeStamp = obj2.RegistrationTimeStamp
                                                                                                            })
                                                                                            .Where(obj2 => obj2.AppointmentTimeStamp.Date == date)
                                                                     })
                                                      .ToListAsync()
                                                      .ConfigureAwait(false);

                        foreach (var channel in channels)
                        {
                            foreach (var appointmentRegistrations in channel.Registrations.GroupBy(obj => obj.AppointmentTimeStamp))
                            {
                                var registrationsStack = new Stack<AppointmentCreationRegistrationData>(appointmentRegistrations.OrderByDescending(obj => obj.RegistrationTimeStamp));
                                while (registrationsStack.Count >= 5)
                                {
                                    var registrations = new List<AppointmentCreationRegistrationData>();
                                    for (var i = 0; i < 5; i++)
                                    {
                                        registrations.Add(registrationsStack.Pop());
                                    }

                                    var messageId = await builder.CreateAppointmentMessage(channel.ChannelId, appointmentRegistrations.Key, registrations)
                                                                 .ConfigureAwait(false);

                                    var appointment = new FractalAppointmentEntity
                                                      {
                                                          ConfigurationId = channel.ConfigurationId,
                                                          MessageId = messageId,
                                                          AppointmentTimeStamp = appointmentRegistrations.Key
                                                      };

                                    if (dbFactory.GetRepository<FractalAppointmentRepository>()
                                                 .Add(appointment))
                                    {
                                        foreach (var registration in registrations)
                                        {
                                            dbFactory.GetRepository<FractalRegistrationRepository>()
                                                     .Refresh(obj => obj.ConfigurationId == channel.ConfigurationId
                                                                  && obj.AppointmentTimeStamp == appointmentRegistrations.Key
                                                                  && obj.UserId == registration.UserId,
                                                              obj => obj.AppointmentId = appointment.Id);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion // AsyncJob
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.EntityFrameworkCore;

using Scruffy.Commands.Base;
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.CoreData;
using Scruffy.Services.Raid;
using Scruffy.Services.Raid.DialogElements;
using Scruffy.Services.Raid.DialogElements.Forms;

namespace Scruffy.Commands.Raid
{
    /// <summary>
    /// Raid commands
    /// </summary>
    [Group("raid")]
    public class RaidCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public RaidCommandModule(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// User management service
        /// </summary>
        public UserManagementService UserManagementService { get; set; }

        /// <summary>
        /// Message builder
        /// </summary>
        public RaidMessageBuilder MessageBuilder { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Starts the setup assistant
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        public async Task Setup(CommandContext commandContext)
        {
            var data = await DialogHandler.RunForm<CreateRaidDayFormData>(commandContext, true)
                                          .ConfigureAwait(false);

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var message = await commandContext.Channel
                                                  .SendMessageAsync("TODO") // TODO
                                                  .ConfigureAwait(false);

                dbFactory.GetRepository<RaidDayConfigurationRepository>()
                         .Add(new RaidDayConfigurationEntity
                               {
                                   AliasName = data.AliasName,
                                   Day = data.Day,
                                   RegistrationDeadline = data.RegistrationDeadline,
                                   StartTime = data.StartTime,
                                   ChannelId = commandContext.Channel.Id,
                                   MessageId = message.Id
                               });
            }
        }

        /// <summary>
        /// Joining an appointment
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="name">Name</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("join")]
        public async Task Join(CommandContext commandContext, string name)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var appointment = await dbFactory.GetRepository<RaidAppointmentRepository>()
                                                 .GetQuery()
                                                 .Where(obj => obj.TimeStamp > DateTime.Now
                                                               && obj.RaidDayConfiguration.AliasName == name)
                                                 .Select(obj => new
                                                                {
                                                                    obj.Id,
                                                                    obj.ConfigurationId
                                                                })
                                                 .FirstOrDefaultAsync()
                                                 .ConfigureAwait(false);
                if (appointment != null)
                {
                    if (dbFactory.GetRepository<RaidRegistrationRepository>()
                                 .AddOrRefresh(obj => obj.AppointmentId == appointment.Id
                                                      && obj.UserId == commandContext.User.Id,
                                               obj =>
                                               {
                                                   if (obj.Id == 0)
                                                   {
                                                       obj.AppointmentId = appointment.Id;
                                                       obj.UserId = commandContext.User.Id;
                                                       obj.RegistrationTimeStamp = DateTime.Now;
                                                   }
                                               }))
                    {
                        await MessageBuilder.RefreshMessageAsync(appointment.ConfigurationId)
                                            .ConfigureAwait(false);
                    }
                }
                else
                {
                    // TODO No current appointment
                }
            }
        }

        /// <summary>
        /// Joining an appointment
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="name">Name</param>
        /// <param name="role">Role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("join")]
#if RELEASE
        [Hidden] // TODO
#endif
        public async Task Join(CommandContext commandContext, string name, string role)
        {
            await Task.Delay(1).ConfigureAwait(false);
        }

        /// <summary>
        /// Leaving an appointment
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="name">Name</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("leave")]
        public async Task Leave(CommandContext commandContext, string name)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var appointment = await dbFactory.GetRepository<RaidAppointmentRepository>()
                                                 .GetQuery()
                                                 .Where(obj => obj.TimeStamp > DateTime.Now
                                                               && obj.RaidDayConfiguration.AliasName == name)
                                                 .Select(obj => new
                                                 {
                                                     obj.Id,
                                                     obj.ConfigurationId
                                                 })
                                                 .FirstOrDefaultAsync()
                                                 .ConfigureAwait(false);
                if (appointment != null)
                {
                    if (dbFactory.GetRepository<RaidRegistrationRepository>()
                                 .Remove(obj => obj.AppointmentId == appointment.Id
                                                      && obj.UserId == commandContext.User.Id))
                    {
                        await MessageBuilder.RefreshMessageAsync(appointment.ConfigurationId)
                                            .ConfigureAwait(false);
                    }
                }
                else
                {
                    // TODO No current appointment
                }
            }
        }

        /// <summary>
        /// Leaving an appointment
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="name">Name</param>
        /// <param name="role">Role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("leave")]
#if RELEASE
        [Hidden] // TODO
#endif
        public async Task Leave(CommandContext commandContext, string name, string role)
        {
            await Task.Delay(1).ConfigureAwait(false);
        }

        #endregion // Methods

        #region Roles

        /// <summary>
        /// Role administration
        /// </summary>
        [Group("roles")]
        public class RaidRolesCommandModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public RaidRolesCommandModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

            #region Properties

            /// <summary>
            /// User management service
            /// </summary>
            public UserManagementService UserManagementService { get; set; }

            /// <summary>
            /// Message builder
            /// </summary>
            public RaidMessageBuilder MessageBuilder { get; set; }

            /// <summary>
            /// Raid roles service
            /// </summary>
            public RaidRolesService RaidRolesService { get; set; }

            #endregion // Properties

            /// <summary>
            /// Starting the personal roles assistant
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("own")]
#if RELEASE
            [Hidden] // TODO
#endif
            public async Task RolesOwn(CommandContext commandContext)
            {
                await Task.Delay(1).ConfigureAwait(false);
            }

            /// <summary>
            /// Starting the roles assistant
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("setup")]
            public async Task SetupRoles(CommandContext commandContext)
            {
                await RaidRolesService.RunAssistantAsync(commandContext)
                                      .ConfigureAwait(false);
            }
        }

        #endregion // Roles

        #region Templates

        /// <summary>
        /// Template administration
        /// </summary>
        [Group("templates")]
        public class RaidTemplatesCommandModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public RaidTemplatesCommandModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

            #region Methods

            /// <summary>
            /// Starting the templates assistant
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("setup")]
            public async Task Setup(CommandContext commandContext)
            {
                bool repeat;

                do
                {
                    repeat = await DialogHandler.Run<RaidTemplateSetupDialogElement, bool>(commandContext).ConfigureAwait(false);
                }
                while (repeat);
            }

            #endregion // Methods
        }

        #endregion // Templates

        #region Levels

        /// <summary>
        /// Template administration
        /// </summary>
        [Group("levels")]
        public class RaidExperienceLevelsCommandModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public RaidExperienceLevelsCommandModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

            #region Methods

            /// <summary>
            /// Starting the experience levels assistant
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("setup")]
            public async Task Setup(CommandContext commandContext)
            {
                bool repeat;

                do
                {
                    repeat = await DialogHandler.Run<RaidExperienceLevelSetupDialogElement, bool>(commandContext)
                                                .ConfigureAwait(false);
                }
                while (repeat);
            }

            #endregion // Methods
        }

        #endregion // Templates
    }
}

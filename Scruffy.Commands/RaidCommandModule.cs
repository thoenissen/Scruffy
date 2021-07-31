using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.CoreData;
using Scruffy.Services.Raid;
using Scruffy.Services.Raid.DialogElements;
using Scruffy.Services.Raid.DialogElements.Forms;

namespace Scruffy.Commands
{
    /// <summary>
    /// Raid commands
    /// </summary>
    [Group("raid")]
    [ModuleLifespan(ModuleLifespan.Transient)]
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

        /// <summary>
        /// Committing a appointment
        /// </summary>
        public RaidCommitService CommitService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Starts the setup assistant
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireAdministratorPermissions]
        public async Task Setup(CommandContext commandContext)
        {
            var data = await DialogHandler.RunForm<CreateRaidDayFormData>(commandContext, true)
                                          .ConfigureAwait(false);

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var message = await commandContext.Channel
                                                  .SendMessageAsync(DiscordEmojiService.GetProgressEmoji(commandContext.Client))
                                                  .ConfigureAwait(false);

                var configuration = new RaidDayConfigurationEntity
                                  {
                                      AliasName = data.AliasName,
                                      Day = data.Day,
                                      RegistrationDeadline = data.RegistrationDeadline,
                                      StartTime = data.StartTime,
                                      ChannelId = commandContext.Channel.Id,
                                      MessageId = message.Id
                                  };

                dbFactory.GetRepository<RaidDayConfigurationRepository>()
                         .Add(configuration);

                var appointmentTimeStamp = DateTime.Today.Add(data.StartTime);
                while (appointmentTimeStamp < DateTime.Now
                    || appointmentTimeStamp.DayOfWeek != data.Day)
                {
                    appointmentTimeStamp = appointmentTimeStamp.AddDays(1);
                }

                dbFactory.GetRepository<RaidAppointmentRepository>()
                         .Add(new RaidAppointmentEntity
                              {
                                  ConfigurationId = configuration.Id,
                                  TemplateId = data.TemplateId,
                                  TimeStamp = appointmentTimeStamp,
                                  Deadline = appointmentTimeStamp.Date
                                                                 .Add(data.RegistrationDeadline)
                              });

                await MessageBuilder.RefreshMessageAsync(configuration.Id)
                                    .ConfigureAwait(false);
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
                    await commandContext.RespondAsync(LocalizationGroup.GetText("NoActiveAppointment", "Currently there is no active appointment."))
                                        .ConfigureAwait(false);
                }
            }
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
                    dbFactory.GetRepository<RaidRegistrationRoleAssignmentRepository>()
                             .RemoveRange(obj => obj.RaidRegistration.AppointmentId == appointment.Id);

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
                    await commandContext.RespondAsync(LocalizationGroup.GetText("NoActiveAppointment", "Currently there is no active appointment."))
                                        .ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Commiting the current raid appointment
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <param name="aliasName">Alias name</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("commit")]
        [RequireAdministratorPermissions]
        public async Task Commit(CommandContext commandContext, string aliasName)
        {
            await CommitService.CommitRaidAppointment(commandContext, aliasName)
                               .ConfigureAwait(false);
        }

        #endregion // Methods

        #region Roles

        /// <summary>
        /// Role administration
        /// </summary>
        [Group("roles")]
        [ModuleLifespan(ModuleLifespan.Transient)]
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

            #region Methods

            /// <summary>
            /// Starting the roles assistant
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("setup")]
            [RequireAdministratorPermissions]
            public async Task SetupRoles(CommandContext commandContext)
            {
                await RaidRolesService.RunAssistantAsync(commandContext)
                                      .ConfigureAwait(false);
            }

            #endregion // Methods
        }

        #endregion // Roles

        #region Templates

        /// <summary>
        /// Template administration
        /// </summary>
        [Group("templates")]
        [ModuleLifespan(ModuleLifespan.Transient)]
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
            [RequireAdministratorPermissions]
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
        [ModuleLifespan(ModuleLifespan.Transient)]
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

            #region Properties

            /// <summary>
            /// Experience level service
            /// </summary>
            public RaidExperienceLevelsService RaidExperienceLevelsService { get; set; }

            #endregion // Properties

            #region Methods

            /// <summary>
            /// Starting the experience levels assistant
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("setup")]
            [RequireAdministratorPermissions]
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

            /// <summary>
            /// Set experience levels to players
            /// </summary>
            /// <param name="commandContext">Command context</param>
            /// <param name="aliasName">Alias name</param>
            /// <param name="users">Users</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("set")]
            [RequireAdministratorPermissions]
            public async Task SetExperienceLevel(CommandContext commandContext, string aliasName, params DiscordUser[] users)
            {
                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    var experienceLevelId = await dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                           .GetQuery()
                                                           .Where(obj => obj.AliasName == aliasName)
                                                           .Select(obj => obj.Id)
                                                           .FirstOrDefaultAsync()
                                                           .ConfigureAwait(false);

                    if (experienceLevelId > 0)
                    {
                        foreach (var user in users)
                        {
                            dbFactory.GetRepository<UserRepository>()
                                     .AddOrRefresh(obj => obj.Id == user.Id,
                                                   obj =>
                                                   {
                                                       if (obj.Id == default)
                                                       {
                                                           obj.Id = user.Id;
                                                           obj.CreationTimeStamp = DateTime.Now;
                                                       }

                                                       obj.RaidExperienceLevelId = experienceLevelId;
                                                   });
                        }

                        await commandContext.Message
                                            .CreateReactionAsync(DiscordEmojiService.GetCheckEmoji(commandContext.Client))
                                            .ConfigureAwait(false);
                    }
                    else
                    {
                        await commandContext.RespondAsync(LocalizationGroup.GetText("UnknownExperienceLevel", "The experience role by the given name does not exist."))
                                            .ConfigureAwait(false);
                    }
                }
            }

            /// <summary>
            /// Post overview of experience roles
            /// </summary>
            /// <param name="commandContext">Command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("overview")]
            public async Task SendExperienceLevelOverview(CommandContext commandContext)
            {
                await RaidExperienceLevelsService.PostExperienceLevelOverview(commandContext)
                                                 .ConfigureAwait(false);
            }

            #endregion // Methods
        }

        #endregion // Templates
    }
}

using Discord;
using Discord.Commands;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.CoreData;
using Scruffy.Data.Entity.Tables.Discord;
using Scruffy.Data.Enumerations.CoreData;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;
using Scruffy.Services.Raid;
using Scruffy.Services.Raid.DialogElements;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Raid commands
/// </summary>
[Group("raid")]
[Alias("ra")]
[BlockedChannelCheck]
public class RaidCommandModule : LocatedTextCommandModuleBase
{
    #region Methods

    /// <summary>
    /// Starts the setup assistant
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setup")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task Setup() => ShowMigrationMessage("raid-admin setup");

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("join")]
    [RequireContext(ContextType.Guild)]
    public Task Join(string name) => ShowMigrationMessage("raid join");

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("joinUser")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task Join(IGuildUser user, string name) => ShowMigrationMessage("raid-admin join-user");

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("leave")]
    [RequireContext(ContextType.Guild)]
    public Task Leave(string name) => ShowMigrationMessage("raid leave");

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("leaveUser")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task Leave(IGuildUser user, string name) => ShowMigrationMessage("raid-admin leave-user");

    /// <summary>
    /// Commiting the current raid appointment
    /// </summary>
    /// <param name="name">Alias name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setTemplate")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task SetTemplate(string name) => ShowMigrationMessage("raid-admin set-template");

    /// <summary>
    /// Commiting the current raid appointment
    /// </summary>
    /// <param name="name">Alias name</param>
    /// <param name="count">Count</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setGroupCount")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task SetGroupCount(string name, int count) => ShowMigrationMessage("raid-admin set-group-count");

    /// <summary>
    /// Commiting the current raid appointment
    /// </summary>
    /// <param name="aliasName">Alias name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("commit")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public Task Commit(string aliasName) => ShowMigrationMessage("raid-admin commit");

    /// <summary>
    /// Daily logs
    /// </summary>
    /// <param name="day">Day</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("logs")]
    public Task Logs(string day = null) => ShowMigrationMessage("raid logs");

    /// <summary>
    /// Post guides overview
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("guides")]
    public Task Guides() => ShowMigrationMessage("raid guides");

    #endregion // Methods

    #region Roles

    /// <summary>
    /// Role administration
    /// </summary>
    [Group("roles")]
    [Alias("r")]
    public class RaidRolesCommandModule : LocatedTextCommandModuleBase
    {
        #region Properties

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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public async Task SetupRoles()
        {
            await RaidRolesService.RunAssistantAsync(Context)
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
    [Alias("t")]
    public class RaidTemplatesCommandModule : LocatedTextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Starting the templates assistant
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public async Task Setup()
        {
            bool repeat;

            do
            {
                repeat = await DialogHandler.Run<RaidTemplateSetupDialogElement, bool>(Context)
                                            .ConfigureAwait(false);
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
    [Alias("l", "level")]
    public class RaidExperienceLevelsCommandModule : LocatedTextCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Experience level service
        /// </summary>
        public RaidExperienceLevelsService RaidExperienceLevelsService { get; set; }

        /// <summary>
        /// Raid registration
        /// </summary>
        public RaidRegistrationService RaidRegistrationService { get; set; }

        /// <summary>
        /// Message builder
        /// </summary>
        public RaidMessageBuilder MessageBuilder { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Starting the experience levels assistant
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public async Task Setup()
        {
            bool repeat;

            do
            {
                repeat = await DialogHandler.Run<RaidExperienceLevelSetupDialogElement, bool>(Context)
                                            .ConfigureAwait(false);
            }
            while (repeat);
        }

        /// <summary>
        /// Set experience levels to players
        /// </summary>
        /// <param name="aliasName">Alias name</param>
        /// <param name="discordUsers">Users</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("set")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public async Task SetExperienceLevel(string aliasName, params IGuildUser[] discordUsers)
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
                    var changedRanks = new List<(IGuildUser User, long? OldExperienceLevelId, long NewExperienceLevelId)>();

                    foreach (var discordUser in discordUsers)
                    {
                        if (dbFactory.GetRepository<UserRepository>()
                                     .Refresh(obj => obj.DiscordAccounts.Any(obj2 => obj2.Id == discordUser.Id),
                                              obj =>
                                              {
                                                  if (obj.RaidExperienceLevelId != experienceLevelId)
                                                  {
                                                      changedRanks.Add((discordUser,
                                                                        obj.RaidExperienceLevelId, experienceLevelId));
                                                  }

                                                  obj.RaidExperienceLevelId = experienceLevelId;
                                              })
                         == false)
                        {
                            var user = new UserEntity
                                       {
                                           CreationTimeStamp = DateTime.Now,
                                           Type = UserType.DiscordUser,
                                           RaidExperienceLevelId = experienceLevelId
                                       };

                            if (dbFactory.GetRepository<UserRepository>()
                                         .Add(user))
                            {
                                dbFactory.GetRepository<DiscordAccountRepository>()
                                         .Add(new DiscordAccountEntity
                                              {
                                                  Id = discordUser.Id,
                                                  UserId = user.Id
                                              });

                                changedRanks.Add((discordUser, null, experienceLevelId));
                            }
                        }
                    }

                    var now = DateTime.Now;

                    var discordUserIds = discordUsers.Select(obj => obj.Id)
                                                     .ToList();

                    var appointments = dbFactory.GetRepository<RaidAppointmentRepository>()
                                                .GetQuery()
                                                .Where(obj => obj.IsCommitted == false
                                                           && obj.TimeStamp > now
                                                           && obj.RaidRegistrations.Any(obj2 => obj2.User.DiscordAccounts.Any(obj3 => discordUserIds.Contains(obj3.Id))))
                                                .Select(obj => new
                                                               {
                                                                   obj.Id,
                                                                   obj.ConfigurationId
                                                               })
                                                .ToList();

                    foreach (var appointment in appointments)
                    {
                        await RaidRegistrationService.RefreshAppointment(appointment.Id)
                                                     .ConfigureAwait(false);

                        await MessageBuilder.RefreshMessageAsync(appointment.ConfigurationId)
                                            .ConfigureAwait(false);
                    }

                    await Context.Message
                                 .DeleteAsync()
                                 .ConfigureAwait(false);

                    var experienceLevels = dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                    .GetQuery()
                                                    .Select(obj => new
                                                                   {
                                                                       obj.Id,
                                                                       obj.Description,
                                                                       obj.Rank
                                                                   })
                                                    .ToList();

                    foreach (var changedRank in changedRanks.Where(obj => obj.NewExperienceLevelId != obj.OldExperienceLevelId))
                    {
                        var oldRank = experienceLevels.FirstOrDefault(obj => obj.Id == changedRank.OldExperienceLevelId);
                        var newRank = experienceLevels.FirstOrDefault(obj => obj.Id == changedRank.NewExperienceLevelId);

                        await Context.Channel
                                     .SendMessageAsync(LocalizationGroup.GetFormattedText("NewRankFormat",
                                                                                          "Level changed for {0}: {1} {2} {3}",
                                                                                          changedRank.User.Mention,
                                                                                          oldRank?.Description,
                                                                                          oldRank == null || oldRank.Rank > newRank?.Rank
                                                                                              ? DiscordEmoteService.GetArrowUpEmote(Context.Client)
                                                                                              : DiscordEmoteService.GetArrowDownEmote(Context.Client),
                                                                                          newRank?.Description))
                                     .ConfigureAwait(false);
                    }
                }
                else
                {
                    await Context.Message
                                 .ReplyAsync(LocalizationGroup.GetText("UnknownExperienceLevel", "The experience role by the given name does not exist."))
                                 .ConfigureAwait(false);
                }
            }
        }

        #endregion // Methods
    }

    #endregion // Templates

    #region Overview

    /// <summary>
    /// Overviews
    /// </summary>
    [Group("overview")]
    [Alias("o")]
    public class RaidOverviewCommandModule : LocatedTextCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Experience level service
        /// </summary>
        public RaidExperienceLevelsService RaidExperienceLevelsService { get; set; }

        /// <summary>
        /// Overviews service
        /// </summary>
        public RaidOverviewService RaidOverviewService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Post overview of participation points
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("participation")]
        [RequireContext(ContextType.Guild)]
        public async Task PostParticipationOverview()
        {
            await RaidOverviewService.PostParticipationOverview(Context)
                                     .ConfigureAwait(false);
        }

        /// <summary>
        /// Post overview of experience roles
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("levels")]
        [RequireContext(ContextType.Guild)]
        public async Task PostExperienceLevelOverview()
        {
            await RaidExperienceLevelsService.PostExperienceLevelOverview(Context)
                                             .ConfigureAwait(false);
        }

        #endregion // Methods
    }

    #endregion // Overview
}
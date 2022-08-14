using System.Data;

using Discord;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.CoreData;
using Scruffy.Data.Entity.Tables.Discord;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Data.Enumerations.CoreData;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Interfaces;
using Scruffy.Services.Raid.DialogElements;
using Scruffy.Services.Raid.DialogElements.Forms;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Raid;

/// <summary>
/// Handling raid commands
/// </summary>
public class RaidCommandHandler : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Message builder
    /// </summary>
    private readonly RaidMessageBuilder _messageBuilder;

    /// <summary>
    /// Committing a appointment
    /// </summary>
    private readonly RaidCommitService _commitService;

    /// <summary>
    /// Registration service
    /// </summary>
    private readonly RaidRegistrationService _registrationService;

    /// <summary>
    /// Role assignment
    /// </summary>
    private readonly RaidRoleAssignmentService _roleAssignmentService;

    /// <summary>
    /// Overview service
    /// </summary>
    private readonly RaidOverviewService _overviewService;

    /// <summary>
    /// Experience levels service
    /// </summary>
    private readonly RaidExperienceLevelsService _experienceLevelsService;

    /// <summary>
    /// User management service
    /// </summary>
    private readonly UserManagementService _userManagementService;

    /// <summary>
    /// Raid Roles Service
    /// </summary>
    private readonly RaidRolesService _raidRolesService;

    /// <summary>
    /// LocalizationService
    /// </summary>
    private readonly LocalizationService _localizationService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="messageBuilder">Message builder</param>
    /// <param name="commitService">Commit service</param>
    /// <param name="registrationService">Registration service</param>
    /// <param name="roleAssignmentService">Role assignment service</param>
    /// <param name="overviewService">Overview service</param>
    /// <param name="experienceLevelsService">Experience levels service</param>
    /// <param name="userManagementService">User management service</param>
    /// <param name="rolesService">Roles Service connector</param>
    public RaidCommandHandler(LocalizationService localizationService,
                              RaidMessageBuilder messageBuilder,
                              RaidCommitService commitService,
                              RaidRegistrationService registrationService,
                              RaidRoleAssignmentService roleAssignmentService,
                              RaidOverviewService overviewService,
                              RaidExperienceLevelsService experienceLevelsService,
                              UserManagementService userManagementService,
                              RaidRolesService rolesService)
        : base(localizationService)
    {
        _localizationService = localizationService;
        _messageBuilder = messageBuilder;
        _commitService = commitService;
        _registrationService = registrationService;
        _roleAssignmentService = roleAssignmentService;
        _overviewService = overviewService;
        _experienceLevelsService = experienceLevelsService;
        _userManagementService = userManagementService;
        _raidRolesService = rolesService;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Starts the appoint configuration assistant
    /// </summary>
    /// <param name="container">Context container</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task AppointmentConfiguration(IContextContainer container)
    {
        var data = await DialogHandler.RunForm<CreateRaidDayFormData>(container, true)
                                      .ConfigureAwait(false);

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var message = await container.Channel
                                       .SendMessageAsync(DiscordEmoteService.GetLoadingEmote(container.Client).ToString())
                                       .ConfigureAwait(false);

            var configuration = new RaidDayConfigurationEntity
                                {
                                    AliasName = data.AliasName,
                                    Day = data.Day,
                                    RegistrationDeadline = data.RegistrationDeadline,
                                    StartTime = data.StartTime,
                                    DiscordChannelId = container.Channel.Id,
                                    DiscordMessageId = message.Id
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
                                                                      .Add(data.RegistrationDeadline),
                                  GroupCount = 1
                              });

            await _messageBuilder.RefreshMessageAsync(configuration.Id)
                                 .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Starts the levels configuration assistant
    /// </summary>
    /// <param name="container">Context container</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task LevelsConfiguration(IContextContainer container)
    {
        bool repeat;

        do
        {
            repeat = await DialogHandler.Run<RaidExperienceLevelSetupDialogElement, bool>(container)
                                        .ConfigureAwait(false);
        }
        while (repeat);
    }

    /// <summary>
    /// Starts the template configuration assistant
    /// </summary>
    /// <param name="container">Context container</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task TemplatesConfiguration(IContextContainer container)
    {
        bool repeat;

        do
        {
            repeat = await DialogHandler.Run<RaidTemplateSetupDialogElement, bool>(container)
                                        .ConfigureAwait(false);
        }
        while (repeat);
    }

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="container">Context container</param>
    /// <param name="name">Name</param>
    /// <param name="isDisplayRoleSelection">Should the role selection be displayed?</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task Join(IContextContainer container, string name, bool isDisplayRoleSelection)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var discordAccountQuery = dbFactory.GetRepository<DiscordAccountRepository>()
                                               .GetQuery()
                                               .Select(obj => obj);

            if (dbFactory.GetRepository<RaidUserRoleRepository>()
                         .GetQuery()
                         .Any(obj => discordAccountQuery.Any(obj2 => obj2.UserId == obj.UserId
                                                                  && obj2.Id == container.User.Id)))
            {
                var appointment = await dbFactory.GetRepository<RaidAppointmentRepository>()
                                                 .GetQuery()
                                                 .Where(obj => obj.TimeStamp > DateTime.Now
                                                            && obj.RaidDayConfiguration.AliasName == name)
                                                 .Select(obj => new
                                                 {
                                                     obj.Id,
                                                     obj.ConfigurationId,
                                                     obj.Deadline
                                                 })
                                                 .FirstOrDefaultAsync()
                                                 .ConfigureAwait(false);

                if (appointment != null)
                {
                    var registrationId = await _registrationService.Join(container, appointment.Id, container.User)
                                                                   .ConfigureAwait(false);

                    if (registrationId != null)
                    {
                        if (isDisplayRoleSelection)
                        {
                            if (DateTime.Now < appointment.Deadline)
                            {
                                await _roleAssignmentService.AssignRoles(container, registrationId.Value)
                                                            .ConfigureAwait(false);
                            }
                            else
                            {
                                await container.ReplyAsync(LocalizationGroup.GetText("NoRoleSelectionAfterDeadline", "It is not possible to edit your preferred roles after the registration deadline."), ephemeral: true)
                                               .ConfigureAwait(false);
                            }
                        }

                        await _messageBuilder.RefreshMessageAsync(appointment.ConfigurationId)
                                             .ConfigureAwait(false);
                    }
                }
                else
                {
                    await container.ReplyAsync(LocalizationGroup.GetText("NoActiveAppointment", "Currently there is no active appointment."), ephemeral: true)
                                   .ConfigureAwait(false);
                }
            }
            else
            {
                await ConfigureRolesFirstTime(container).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Role selection
    /// </summary>
    /// <param name="container">Context container</param>
    /// <param name="configurationId">Configuration id</param>
    /// <param name="values">Roles</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task SelectRoles(InteractionContextContainer container, long configurationId, string[] values)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var discordAccountQuery = dbFactory.GetRepository<DiscordAccountRepository>()
                                               .GetQuery()
                                               .Select(obj => obj);

            if (dbFactory.GetRepository<RaidUserRoleRepository>()
                         .GetQuery()
                         .Any(obj => discordAccountQuery.Any(obj2 => obj2.UserId == obj.UserId
                                                                  && obj2.Id == container.User.Id)))
            {
                var appointment = await dbFactory.GetRepository<RaidAppointmentRepository>()
                                             .GetQuery()
                                             .Where(obj => obj.TimeStamp > DateTime.Now
                                                        && obj.ConfigurationId == configurationId)
                                             .Select(obj => new
                                             {
                                                 obj.Id,
                                                 obj.ConfigurationId,
                                                 obj.Deadline
                                             })
                                             .FirstOrDefaultAsync()
                                             .ConfigureAwait(false);

                if (appointment != null)
                {
                    var registrationId = await _registrationService.Join(container, appointment.Id, container.User)
                                                                   .ConfigureAwait(false);

                    if (registrationId != null)
                    {
                        if (DateTime.Now < appointment.Deadline)
                        {
                            dbFactory.GetRepository<RaidRegistrationRoleAssignmentRepository>()
                                     .RemoveRange(obj => obj.RegistrationId == registrationId);

                            if (values?.Length > 0
                             && values.Length < 8)
                            {
                                foreach (var roleId in values.Select(obj => Convert.ToInt64(obj)))
                                {
                                    dbFactory.GetRepository<RaidRegistrationRoleAssignmentRepository>()
                                             .Add(new RaidRegistrationRoleAssignmentEntity
                                             {
                                                 RegistrationId = registrationId.Value,
                                                 RoleId = roleId
                                             });
                                }
                            }
                        }
                        else
                        {
                            await container.ReplyAsync(LocalizationGroup.GetText("NoRoleSelectionAfterDeadline", "It is not possible to edit your preferred roles after the registration deadline."), ephemeral: true)
                                           .ConfigureAwait(false);
                        }

                        await _messageBuilder.RefreshMessageAsync(appointment.ConfigurationId)
                                             .ConfigureAwait(false);
                    }
                }
                else
                {
                    await container.ReplyAsync(LocalizationGroup.GetText("NoActiveAppointment", "Currently there is no active appointment."), ephemeral: true)
                                   .ConfigureAwait(false);
                }
            }
            else
            {
                await ConfigureRolesFirstTime(container).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="container">Context container</param>
    /// <param name="user">User</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task Join(IContextContainer container, IGuildUser user, string name)
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
                var registrationId = await _registrationService.Join(container, appointment.Id, user)
                                                               .ConfigureAwait(false);

                if (registrationId != null)
                {
                    await _roleAssignmentService.AssignRoles(container, registrationId.Value)
                                                .ConfigureAwait(false);

                    await _messageBuilder.RefreshMessageAsync(appointment.ConfigurationId)
                                         .ConfigureAwait(false);
                }
            }
            else
            {
                await container.ReplyAsync(LocalizationGroup.GetText("NoActiveAppointment", "Currently there is no active appointment."))
                               .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="container">Context container</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task<bool> Leave(IContextContainer container, string name)
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
                var user = await _userManagementService.GetUserByDiscordAccountId(container.User)
                                                       .ConfigureAwait(false);

                if (await _registrationService.Leave(appointment.Id, user.Id)
                                              .ConfigureAwait(false))
                {
                    await _messageBuilder.RefreshMessageAsync(appointment.ConfigurationId)
                                         .ConfigureAwait(false);
                }

                return true;
            }

            await container.ReplyAsync(LocalizationGroup.GetText("NoActiveAppointment", "Currently there is no active appointment."))
                           .ConfigureAwait(false);

            return false;
        }
    }

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="container">Context container</param>
    /// <param name="user">User</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task Leave(IContextContainer container, IGuildUser user, string name)
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
                var internalUser = await _userManagementService.GetUserByDiscordAccountId(user)
                                                               .ConfigureAwait(false);

                if (await _registrationService.Leave(appointment.Id, internalUser.Id)
                                              .ConfigureAwait(false))
                {
                    await _messageBuilder.RefreshMessageAsync(appointment.ConfigurationId)
                                         .ConfigureAwait(false);
                }
            }
            else
            {
                await container.ReplyAsync(LocalizationGroup.GetText("NoActiveAppointment", "Currently there is no active appointment."))
                               .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Commiting the current raid appointment
    /// </summary>
    /// <param name="container">Context container</param>
    /// <param name="name">Alias name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task SetTemplate(IContextContainer container, string name)
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
                if (await _registrationService.SetTemplate(container, appointment.Id)
                                              .ConfigureAwait(false))
                {
                    await _messageBuilder.RefreshMessageAsync(appointment.ConfigurationId)
                                         .ConfigureAwait(false);

                    await container.SendMessageAsync(LocalizationGroup.GetText("TemplateChanged", "The template has been changed."))
                                   .ConfigureAwait(false);
                }
            }
            else
            {
                await container.ReplyAsync(LocalizationGroup.GetText("NoActiveAppointment", "Currently there is no active appointment."))
                               .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Commiting the current raid appointment
    /// </summary>
    /// <param name="container">Context container</param>
    /// <param name="name">Alias name</param>
    /// <param name="count">Count</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task SetGroupCount(IContextContainer container, string name, int count)
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
                if (await _registrationService.SetGroupCount(appointment.Id, count)
                                              .ConfigureAwait(false))
                {
                    await _messageBuilder.RefreshMessageAsync(appointment.ConfigurationId)
                                         .ConfigureAwait(false);

                    await container.ReplyAsync(LocalizationGroup.GetText("GroupCountChanged", "The group count has been changed."))
                                   .ConfigureAwait(false);
                }
            }
            else
            {
                await container.ReplyAsync(LocalizationGroup.GetText("NoActiveAppointment", "Currently there is no active appointment."))
                               .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Commiting the current raid appointment
    /// </summary>
    /// <param name="container">Context container</param>
    /// <param name="aliasName">Alias name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task Commit(IContextContainer container, string aliasName)
    {
        await _commitService.CommitRaidAppointment(container, aliasName)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Post guides overview
    /// </summary>
    /// <param name="container">Context container</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task Guides(IContextContainer container)
    {
        var builder = new EmbedBuilder().WithTitle(LocalizationGroup.GetText("RaidGuides", "Raid guides"))
                                        .WithColor(Color.Green)
                                        .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                        .WithTimestamp(DateTime.Now);

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848910035747864576)} {Format.Url("Vale Guardian", "https://wiki.guildwars2.com/wiki/Vale_Guardian")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=qtzMDCVHlLg")} - {Format.Url("Tekkit", "https://www.youtube.com/watch?v=IiSsq85T23Q")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 743938320459104317)} {Format.Url("Spirit Woods", "https://wiki.guildwars2.com/wiki/Traverse_the_Spirit_Woods")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=Ni_-HkRtmm4")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908993538949131)} {Format.Url("Gorseval the Multifarious", "https://wiki.guildwars2.com/wiki/Gorseval_the_Multifarious")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=B9qbuP9kv4Q")} - {Format.Url("Tekkit", "https://www.youtube.com/watch?v=J6SnUnsppPc")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909543915651072)} {Format.Url("Sabetha the Saboteur", "https://wiki.guildwars2.com/wiki/Sabetha_the_Saboteur")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=4bqwSrZ2Sr4")} - {Format.Url("Tekkit", "https://www.youtube.com/watch?v=yFo5QPoZ1aM")})");
        builder.AddField("Wing 1 - Spirit Vale", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909627982610482)} {Format.Url("Slothasor", "https://wiki.guildwars2.com/wiki/Slothasor")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=WIBTTAPd3ME")} - {Format.Url("Tekkit", "https://www.youtube.com/watch?v=pWO-9zdVJlc")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909882115358720)} {Format.Url("Bandit Trio", "https://wiki.guildwars2.com/wiki/Protect_the_caged_prisoners")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=ncVYseut8ag")} - {Format.Url("Tekkit", "https://www.youtube.com/watch?v=wEMMRwoM56Q")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909162821845043)} {Format.Url("Matthias Gabrel", "https://wiki.guildwars2.com/wiki/Matthias_Gabrel")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=q99ocDpSnAI")})");
        builder.AddField("Wing 2 - Salvation Pass", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 743938372195844117)} {Format.Url("Siege the Stronghold", "https://wiki.guildwars2.com/wiki/Siege_the_Stronghold")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=WFwcnj74lwU")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909049599885322)} {Format.Url("Keep Construct", "https://wiki.guildwars2.com/wiki/Keep_Construct")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=QIdzfDtxaFQ")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909953112473622)} {Format.Url("Twisted Castle", "https://wiki.guildwars2.com/wiki/Traverse_the_Twisted_Castle")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=mRwLCQ7rUTw")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848910090370940949)} {Format.Url("Xera", "https://wiki.guildwars2.com/wiki/Xera")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=Iggsq-f3yBw")})");
        builder.AddField("Wing 3 - Stronghold of the Faithful", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908521680142359)} {Format.Url("Cairn the Indomitable", "https://wiki.guildwars2.com/wiki/Cairn_the_Indomitable")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=zPfLE1wwWkw")} - {Format.Url("Tekkit", "https://www.youtube.com/watch?v=SjzG0qEi20o")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909340827713557)} {Format.Url("Mursaat Overseer", "https://wiki.guildwars2.com/wiki/Mursaat_Overseer")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=iHMzAPWoLDU")} - {Format.Url("Tekkit", "https://www.youtube.com/watch?v=pcYEEX6RgDM")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909587938803762)} {Format.Url("Samarog", "https://wiki.guildwars2.com/wiki/Samarog")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=pLudL0RBgf0")} - {Format.Url("Tekkit", "https://www.youtube.com/watch?v=diwPG9Dsrt8")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908773996101642)} {Format.Url("Deimos", "https://wiki.guildwars2.com/wiki/Deimos")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=gW5HH36X3zU")} - {Format.Url("Tekkit", "https://www.youtube.com/watch?v=04KJ9Mtocmw")})");
        builder.AddField("Wing 4 - Bastion of the Penitent", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848911345964679188)} {Format.Url("Soulless Horror", "https://wiki.guildwars2.com/wiki/Soulless_Horror")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=Nw6D9_RcJhs")} - {Format.Url("Tekkit", "https://www.youtube.com/watch?v=FquU1cnv1Xc")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 743940484455596064)} {Format.Url("River of Souls", "https://wiki.guildwars2.com/wiki/Traverse_the_River_of_Souls")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=-84PZQGCmyI")} - {Format.Url("Tekkit", "https://www.youtube.com/watch?v=gCC8EjHHMzE")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909739509547058)}{DiscordEmoteService.GetGuildEmote(container.Client, 848908876039585822)}{DiscordEmoteService.GetGuildEmote(container.Client, 848908317832773692)} {Format.Url("Statues of Grenth", "https://wiki.guildwars2.com/wiki/Restore_the_three_statues_of_Grenth")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=cKUcf_ez9Ec")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908828866379777)} {Format.Url("Dhuum", "https://wiki.guildwars2.com/wiki/Dhuum")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=5gFxLKJI9ds")} - {Format.Url("Tekkit", "https://www.youtube.com/watch?v=uEfAH8DdaUg")})");
        builder.AddField("Wing 5 - Hall of Chains", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908712692547614)} {Format.Url("Conjured Amalgamate", "https://wiki.guildwars2.com/wiki/Conjured_Amalgamate")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=B7VQAZuV-6o")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909098619895808)} {Format.Url("Twin Largos", "https://wiki.guildwars2.com/wiki/Defeat_the_twin_largos")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=RUQQhyE_YiA")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909410691973140)} {Format.Url("Qadim", "https://wiki.guildwars2.com/wiki/Qadim")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=nU_yQtk_8oo")})");
        builder.AddField("Wing 6 - Mythwright Gambit", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908653637533736)} {Format.Url("Cardinal Sabir", "https://wiki.guildwars2.com/wiki/Cardinal_Sabir")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=jwjqrsBQhEo")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908580749049866)} {Format.Url("Cardinal Adina", "https://wiki.guildwars2.com/wiki/Cardinal_Adina")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=T_RwWm0Jj4k")} - {Format.Url("MightyTeapot", "https://www.youtube.com/watch?v=UY1iBmxDD6M")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909465553207296)} {Format.Url("Qadim the Peerless", "https://wiki.guildwars2.com/wiki/Qadim_the_Peerless")} ({Format.Url("Mukluk", "https://www.youtube.com/watch?v=v1ZFeRot8zg")} - {Format.Url("Hepha Cinema", "https://www.youtube.com/watch?v=vUELGR2S2Mc")})");
        builder.AddField("Wing 7 - The Key of Ahdashim", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.Append($"{Format.Url("Xera", "https://cdn.discordapp.com/attachments/847555191842537552/888025983649722399/unknown.png")}");
        stringBuilder.Append(" - ");
        stringBuilder.Append($"{Format.Url("Dhuum", "https://cdn.discordapp.com/attachments/847555191842537552/888031307798556672/5cRRIt5NNdTcj2KUwYrzIbuVxpYTXN7Qknn9lSNhiWw.png")}");
        stringBuilder.Append(" - ");
        stringBuilder.Append($"{Format.Url("Qadim", "https://media.discordapp.net/attachments/847555191842537552/888032050534301706/MRM_FYBHoCRB9diOF_-dNZtAby8XID3Lcnp15OF151U.png?width=1052&height=1052")}");
        stringBuilder.Append(" - ");
        stringBuilder.Append($"{Format.Url("Qadim the Peerless", "https://media.discordapp.net/attachments/847555191842537552/888031828055830588/e_4wRNQcQtoxZ8Nb52T_qZlB0SI-0DhfI5JzNGXA76o.png?width=1052&height=624")}");
        builder.AddField("Markers / Reference sheets", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{Format.Url("Snow Crows", "https://snowcrows.com/")} - {Format.Url("Lucky Noobs", "https://lucky-noobs.com/")} - {Format.Url("Hardstuck", "https://hardstuck.gg/")}");
        builder.AddField("Builds", stringBuilder.ToString());

        await container.ReplyAsync(embed: builder.Build())
                       .ConfigureAwait(false);
    }

    /// <summary>
    /// Set experience levels to players
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="aliasName">Alias name</param>
    /// <param name="discordUsers">Users</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task SetExperienceLevel(InteractionContextContainer context, string aliasName, params IGuildUser[] discordUsers)
    {
        var deferMessage = await context.DeferProcessing()
                                        .ConfigureAwait(false);

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
                                           RaidExperienceLevelId = experienceLevelId,
                                           UserName = $"{discordUser.Username}#{discordUser.Discriminator}",
                                           SecurityStamp = Guid.NewGuid().ToString()
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
                    var transaction = dbFactory.BeginTransaction(IsolationLevel.RepeatableRead);
                    await using (transaction.ConfigureAwait(false))
                    {
                        if (await _registrationService.RefreshAppointment(dbFactory, appointment.Id)
                                                      .ConfigureAwait(false))
                        {
                            await transaction.CommitAsync()
                                             .ConfigureAwait(false);
                        }
                    }

                    await _messageBuilder.RefreshMessageAsync(appointment.ConfigurationId)
                                         .ConfigureAwait(false);
                }

                var experienceLevels = dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                .GetQuery()
                                                .Select(obj => new
                                                {
                                                    obj.Id,
                                                    obj.Description,
                                                    obj.Rank
                                                })
                                                .ToList();

                foreach (var (user, oldExperienceLevelId, newExperienceLevelId) in changedRanks.Where(obj => obj.NewExperienceLevelId != obj.OldExperienceLevelId))
                {
                    var oldRank = experienceLevels.FirstOrDefault(obj => obj.Id == oldExperienceLevelId);
                    var newRank = experienceLevels.FirstOrDefault(obj => obj.Id == newExperienceLevelId);

                    await context.Channel
                                 .SendMessageAsync(LocalizationGroup.GetFormattedText("NewRankFormat",
                                                                                      "Level changed for {0}: {1} {2} {3}",
                                                                                      user.Mention,
                                                                                      oldRank?.Description,
                                                                                      oldRank == null || oldRank.Rank > newRank?.Rank
                                                                                          ? DiscordEmoteService.GetArrowUpEmote(context.Client)
                                                                                          : DiscordEmoteService.GetArrowDownEmote(context.Client),
                                                                                      newRank?.Description))
                                 .ConfigureAwait(false);
                }

                await deferMessage.DeleteAsync()
                                  .ConfigureAwait(false);
            }
            else
            {
                await context.ReplyAsync(LocalizationGroup.GetText("UnknownExperienceLevel", "The experience role by the given name does not exist."), ephemeral: true)
                             .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Post overview of participation points
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task PostParticipationOverview(IContextContainer context)
    {
        await _overviewService.PostParticipationOverview(context)
                              .ConfigureAwait(false);
    }

    /// <summary>
    /// Post overview of experience roles
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task PostExperienceLevelOverview(IContextContainer context)
    {
        await _experienceLevelsService.PostExperienceLevelOverview(context)
                                      .ConfigureAwait(false);
    }

    /// <summary>
    /// Set Raid Ready Roles
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ConfigureRoles(IContextContainer context)
    {
        var dialogHandler = new DialogHandler(context);
        await using (dialogHandler.ConfigureAwait(false))
        {
            var user = await _userManagementService.GetUserByDiscordAccountId(context.User)
                                                   .ConfigureAwait(false);

            var selectedRoles = await dialogHandler.Run<RaidPreparedRolesSelectDialogElement, List<long>>(new RaidPreparedRolesSelectDialogElement(_localizationService, _raidRolesService, _userManagementService))
                                                   .ConfigureAwait(false);

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                dbFactory.GetRepository<RaidUserRoleRepository>()
                         .RemoveRange(obj => obj.UserId == user.Id);

                foreach (var roleId in selectedRoles)
                {
                    dbFactory.GetRepository<RaidUserRoleRepository>()
                             .Add(new RaidUserRoleEntity
                                  {
                                      UserId = user.Id,
                                      RoleId = roleId
                                  });
                }
            }
            await dialogHandler.DeleteMessages()
                               .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// First time role selection
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task ConfigureRolesFirstTime(IContextContainer context)
    {
        var dialogHandler = new DialogHandler(context);
        await using (dialogHandler.ConfigureAwait(false))
        {
            var user = await _userManagementService.GetUserByDiscordAccountId(context.User)
                                                   .ConfigureAwait(false);

            var selectedRoles = await dialogHandler.Run<RaidPreparedRolesFirstTimeSelectDialogElement, List<long>>(new RaidPreparedRolesFirstTimeSelectDialogElement(_localizationService, _raidRolesService))
                                                   .ConfigureAwait(false);

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                dbFactory.GetRepository<RaidUserRoleRepository>()
                         .RemoveRange(obj => obj.UserId == user.Id);

                foreach (var roleId in selectedRoles)
                {
                    dbFactory.GetRepository<RaidUserRoleRepository>()
                             .Add(new RaidUserRoleEntity
                                  {
                                      UserId = user.Id,
                                      RoleId = roleId
                                  });
                }
            }
            await dialogHandler.DeleteMessages()
                               .ConfigureAwait(false);
        }
    }

    #endregion // Methods
}
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

    /// <summary>
    /// Raid line up service
    /// </summary>
    private readonly RaidLineUpService _lineUpService;

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
    /// <param name="lineUpService">Line up service</param>
    public RaidCommandHandler(LocalizationService localizationService,
                              RaidMessageBuilder messageBuilder,
                              RaidCommitService commitService,
                              RaidRegistrationService registrationService,
                              RaidRoleAssignmentService roleAssignmentService,
                              RaidOverviewService overviewService,
                              RaidExperienceLevelsService experienceLevelsService,
                              UserManagementService userManagementService,
                              RaidRolesService rolesService,
                              RaidLineUpService lineUpService)
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
        _lineUpService = lineUpService;
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
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848910035747864576)} {Format.Url("Vale Guardian", "https://bit.ly/2EevcXD")} ({Format.Url("Mukluk", "https://bit.ly/3i7056E")} - {Format.Url("Tekkit", "https://bit.ly/3Xrru3C")} - {Format.Url("Hardstuck", "https://bit.ly/3EuRmCX")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 743938320459104317)} {Format.Url("Spirit Woods", "https://bit.ly/3ViTUuB")} ({Format.Url("Mukluk", "https://bit.ly/3GDDwRd")} - {Format.Url("Hardstuck", "https://bit.ly/3ViFyKL")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908993538949131)} {Format.Url("Gorseval the Multifarious", "https://bit.ly/2EhcXkn")} ({Format.Url("Mukluk", "https://bit.ly/3U4IpWx")} - {Format.Url("Tekkit", "https://bit.ly/3i2B8cE")} - {Format.Url("Hardstuck", "https://bit.ly/3EymNwf")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909543915651072)} {Format.Url("Sabetha the Saboteur", "https://bit.ly/2V2GDaE")} ({Format.Url("Mukluk", "https://bit.ly/3tTBG7u")} - {Format.Url("Tekkit", "https://bit.ly/3XrTlAq")} - {Format.Url("Hardstuck", "https://bit.ly/3EvmaU3")})");
        builder.AddField("Wing 1 - Spirit Vale", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909627982610482)} {Format.Url("Slothasor", "https://bit.ly/2trV77X")} ({Format.Url("Mukluk", "https://bit.ly/3AEFFbD")} - {Format.Url("Tekkit", "https://bit.ly/3gqydKv")} - {Format.Url("Hardstuck", "https://bit.ly/3Ev2zDA")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909882115358720)} {Format.Url("Bandit Trio", "https://bit.ly/2BIN4Z3")} ({Format.Url("Mukluk", "https://bit.ly/3V0eRea")} - {Format.Url("Tekkit", "https://bit.ly/3gna87r")} - {Format.Url("Hardstuck", "https://bit.ly/3EuWt67")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909162821845043)} {Format.Url("Matthias Gabrel", "https://bit.ly/2GuID8a")} ({Format.Url("Mukluk", "https://bit.ly/3tUhcLC")} - {Format.Url("Hardstuck", "https://bit.ly/3EpLmvc")})");
        builder.AddField("Wing 2 - Salvation Pass", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 743938372195844117)} {Format.Url("Siege the Stronghold", "https://bit.ly/2SVjCZz")} ({Format.Url("Mukluk", "https://bit.ly/3US6gdr")} - {Format.Url("Hardstuck", "https://bit.ly/3ErY24R")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909049599885322)} {Format.Url("Keep Construct", "https://bit.ly/2UXydRM")} ({Format.Url("Mukluk", "https://bit.ly/3XqplVD")} - {Format.Url("Hardstuck", "https://bit.ly/3XqKmQk")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909953112473622)} {Format.Url("Twisted Castle", "https://bit.ly/3U0bPFl")} ({Format.Url("Mukluk", "https://bit.ly/3hYpD5T")} - {Format.Url("Hardstuck", "https://bit.ly/3ERgDZs")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848910090370940949)} {Format.Url("Xera", "https://bit.ly/2HYHbsr")} ({Format.Url("Mukluk", "https://bit.ly/3VczljK")} - {Format.Url("Hardstuck", "https://bit.ly/3tUi7M4")})");
        builder.AddField("Wing 3 - Stronghold of the Faithful", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908521680142359)} {Format.Url("Cairn the Indomitable", "https://bit.ly/2SDot2m")} ({Format.Url("Mukluk", "https://bit.ly/3AC2ZXM")} - {Format.Url("Tekkit", "https://bit.ly/3TZ0hlQ")} - {Format.Url("Hardstuck", "https://bit.ly/3Vl51n2")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909340827713557)} {Format.Url("Mursaat Overseer", "https://bit.ly/2NeNsTO")} ({Format.Url("Mukluk", "https://bit.ly/3VmKHBM")} - {Format.Url("Tekkit", "https://bit.ly/3ETo1TZ")} - {Format.Url("Hardstuck", "https://bit.ly/3V1nk0B")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909587938803762)} {Format.Url("Samarog", "https://bit.ly/2V1NWiD")} ({Format.Url("Mukluk", "https://bit.ly/3V3fp34")} - {Format.Url("Tekkit", "https://bit.ly/3GEyNPp")} - {Format.Url("Hardstuck", "https://bit.ly/3EvmKkH")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908773996101642)} {Format.Url("Deimos", "https://bit.ly/2Ij1Mf6")} ({Format.Url("Mukluk", "https://bit.ly/3V6Rr76")} - {Format.Url("Tekkit", "https://bit.ly/3i2BLD2")} - {Format.Url("Hardstuck", "https://bit.ly/3gz8JKN")})");
        builder.AddField("Wing 4 - Bastion of the Penitent", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848911345964679188)} {Format.Url("Soulless Horror", "https://bit.ly/2S80QtF")} ({Format.Url("Mukluk", "https://bit.ly/3EQSOB1")} - {Format.Url("Tekkit", "https://bit.ly/3Vej3Xe")} - {Format.Url("Hardstuck", "https://bit.ly/3XoJQCc")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 743940484455596064)} {Format.Url("River of Souls", "https://bit.ly/3AyqAIv")} ({Format.Url("Mukluk", "https://bit.ly/3tPIBhS")} - {Format.Url("Tekkit", "https://bit.ly/3ACZr7D")} - {Format.Url("Hardstuck", "https://bit.ly/3GFnNBk")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909739509547058)}{DiscordEmoteService.GetGuildEmote(container.Client, 848908876039585822)}{DiscordEmoteService.GetGuildEmote(container.Client, 848908317832773692)} {Format.Url("Statues of Grenth", "https://bit.ly/3tQqcRU")} ({Format.Url("Mukluk", "https://bit.ly/3i7OHYb")} - {Format.Url("Hardstuck", "https://bit.ly/3tTBRiW")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908828866379777)} {Format.Url("Dhuum", "https://bit.ly/3GIXAlz")} ({Format.Url("Mukluk", "https://bit.ly/3tOSk8e")} - {Format.Url("Tekkit", "https://bit.ly/3VojvCF")} - {Format.Url("Hardstuck", "https://bit.ly/3i7oDwB")})");
        builder.AddField("Wing 5 - Hall of Chains", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908712692547614)} {Format.Url("Conjured Amalgamate", "https://bit.ly/2BEAWrX")} ({Format.Url("Mukluk", "https://bit.ly/3i7t76p")} - {Format.Url("Hardstuck", "https://bit.ly/3i4jTI7")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909098619895808)} {Format.Url("Twin Largos", "https://bit.ly/3ACMcU9")} ({Format.Url("Mukluk", "https://bit.ly/3TTta2M")} - {Format.Url("Hardstuck", "https://bit.ly/3gr75eb")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909410691973140)} {Format.Url("Qadim", "https://bit.ly/2V3vAOh")} ({Format.Url("Mukluk", "https://bit.ly/3V1yiTM")} - {Format.Url("Hardstuck", "https://bit.ly/3UVLkCi")})");
        builder.AddField("Wing 6 - Mythwright Gambit", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908653637533736)} {Format.Url("Cardinal Sabir", "https://bit.ly/3XqnD6L")} ({Format.Url("Mukluk", "https://bit.ly/3V1oIAu")} - {Format.Url("Hardstuck", "https://bit.ly/3tOXiSq")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908580749049866)} {Format.Url("Cardinal Adina", "https://bit.ly/3XjsB5n")} ({Format.Url("Mukluk", "https://bit.ly/3AyvQvJ")} - {Format.Url("MightyTeapot", "https://bit.ly/3i4OnJX")} - {Format.Url("Hardstuck", "https://bit.ly/3tUj5YI")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909465553207296)} {Format.Url("Qadim the Peerless", "https://bit.ly/3XqLiUv")} ({Format.Url("Mukluk", "https://bit.ly/3XqLqDt")} - {Format.Url("Hepha Cinema", "https://bit.ly/3EQGznX")} - {Format.Url("Hardstuck", "https://bit.ly/3VhCT43")})");
        builder.AddField("Wing 7 - The Key of Ahdashim", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.Append($"{Format.Url("Xera", "https://bit.ly/3VmPH9t")}");
        stringBuilder.Append(" - ");
        stringBuilder.Append($"{Format.Url("Dhuum", "https://bit.ly/3gwfWvm")}");
        stringBuilder.Append(" - ");
        stringBuilder.Append($"{Format.Url("Qadim", "https://bit.ly/3tSPLlq")}");
        stringBuilder.Append(" - ");
        stringBuilder.Append($"{Format.Url("Qadim the Peerless", "https://bit.ly/3U0dkDt")}");
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
    /// Configure prepared raid roles
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
    /// Configure prepared special raid roles
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ConfigureSpecialRoles(IContextContainer context)
    {
        var dialogHandler = new DialogHandler(context);
        await using (dialogHandler.ConfigureAwait(false))
        {
            var user = await _userManagementService.GetUserByDiscordAccountId(context.User)
                                                   .ConfigureAwait(false);

            var selectedRoles = await dialogHandler.Run<RaidPreparedSpecialRolesSelectDialogElement, List<long>>(new RaidPreparedSpecialRolesSelectDialogElement(_localizationService, _raidRolesService, _userManagementService))
                                                   .ConfigureAwait(false);

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                dbFactory.GetRepository<RaidUserSpecialRoleRepository>()
                         .RemoveRange(obj => obj.UserId == user.Id);

                foreach (var roleId in selectedRoles)
                {
                    dbFactory.GetRepository<RaidUserSpecialRoleRepository>()
                             .Add(new RaidUserSpecialRoleEntity
                                  {
                                      UserId = user.Id,
                                      SpecialRoleId = roleId
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
            try
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
            }
            finally
            {
                await dialogHandler.DeleteMessages()
                                   .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Post current line up
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task PostCurrentLineUp(InteractionContextContainer context)
    {
        var success = false;

        var message = await context.DeferProcessing()
                                   .ConfigureAwait(false);

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var appointmentId = dbFactory.GetRepository<RaidAppointmentRepository>()
                                         .GetQuery()
                                         .Where(obj => obj.IsCommitted == false)
                                         .OrderBy(obj => obj.TimeStamp)
                                         .Select(obj => obj.Id)
                                         .FirstOrDefault();

            if (appointmentId > 0)
            {
                success = await _lineUpService.PostLineUp(appointmentId)
                                              .ConfigureAwait(false);
            }
        }

        if (success)
        {
            await message.DeleteAsync()
                         .ConfigureAwait(false);
        }
        else
        {
            await context.ReplyAsync(LocalizationGroup.GetText("NoLineUpFound", "Current there is no line up to post."))
                         .ConfigureAwait(false);
        }
    }

    #endregion // Methods
}
using System.Net.Http;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.CoreData;
using Scruffy.Data.Entity.Tables.Discord;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Data.Enumerations.CoreData;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Raid;
using Scruffy.Services.Raid.DialogElements;
using Scruffy.Services.Raid.DialogElements.Forms;

namespace Scruffy.Commands;

/// <summary>
/// Raid commands
/// </summary>
[Group("raid")]
[Aliases("ra")]
[ModuleLifespan(ModuleLifespan.Transient)]
public class RaidCommandModule : LocatedCommandModuleBase
{
    #region Fields

    /// <summary>
    /// User management service
    /// </summary>
    private UserManagementService _userManagementService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="userManagementService">User management service</param>
    /// <param name="httpClientFactory">HttpClient-Factory</param>
    public RaidCommandModule(LocalizationService localizationService, UserManagementService userManagementService, IHttpClientFactory httpClientFactory)
        : base(localizationService, userManagementService, httpClientFactory)
    {
        _userManagementService = userManagementService;
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

    /// <summary>
    /// Registration service
    /// </summary>
    public RaidRegistrationService RegistrationService { get; set; }

    /// <summary>
    /// Role assignment
    /// </summary>
    public RaidRoleAssignmentService RoleAssignmentService { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Starts the setup assistant
    /// </summary>
    /// <param name="commandContext">Current command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setup")]
    [RequireGuild]
    [RequireAdministratorPermissions]
    public Task Setup(CommandContext commandContext)
    {
        return InvokeAsync(commandContext,
                           async commandContextContainer =>
                           {
                               var data = await DialogHandler.RunForm<CreateRaidDayFormData>(commandContextContainer, true)
                                                             .ConfigureAwait(false);

                               using (var dbFactory = RepositoryFactory.CreateInstance())
                               {
                                   var message = await commandContextContainer.Channel
                                                                              .SendMessageAsync(DiscordEmojiService.GetProgressEmoji(commandContextContainer.Client))
                                                                              .ConfigureAwait(false);

                                   var configuration = new RaidDayConfigurationEntity
                                                       {
                                                           AliasName = data.AliasName,
                                                           Day = data.Day,
                                                           RegistrationDeadline = data.RegistrationDeadline,
                                                           StartTime = data.StartTime,
                                                           DiscordChannelId = commandContextContainer.Channel.Id,
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
                                                                                    .Add(data.RegistrationDeadline)
                                                 });

                                   await MessageBuilder.RefreshMessageAsync(configuration.Id)
                                                       .ConfigureAwait(false);
                               }
                           });
    }

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="commandContext">Current command context</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("join")]
    [RequireGuild]
    public Task Join(CommandContext commandContext, string name)
    {
        return InvokeAsync(commandContext,
                           async commandContextContainer =>
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
                                       var registrationId = await RegistrationService.Join(commandContextContainer, appointment.Id, commandContextContainer.User.Id)
                                                                                     .ConfigureAwait(false);

                                       if (registrationId != null)
                                       {
                                           await RoleAssignmentService.AssignRoles(commandContextContainer, registrationId.Value)
                                                                      .ConfigureAwait(false);

                                           await MessageBuilder.RefreshMessageAsync(appointment.ConfigurationId)
                                                               .ConfigureAwait(false);

                                           await commandContextContainer.Message
                                                                        .DeleteAsync()
                                                                        .ConfigureAwait(false);
                                       }
                                   }
                                   else
                                   {
                                       await commandContextContainer.Message
                                                                    .RespondAsync(LocalizationGroup.GetText("NoActiveAppointment", "Currently there is no active appointment."))
                                                                    .ConfigureAwait(false);
                                   }
                               }
                           });
    }

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="commandContext">Current command context</param>
    /// <param name="user">User</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("joinUser")]
    [RequireGuild]
    [RequireAdministratorPermissions]
    public Task Join(CommandContext commandContext, DiscordUser user, string name)
    {
        return InvokeAsync(commandContext,
                           async commandContextContainer =>
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
                                       var registrationId = await RegistrationService.Join(commandContextContainer, appointment.Id, user.Id)
                                                                                     .ConfigureAwait(false);

                                       if (registrationId != null)
                                       {
                                           await RoleAssignmentService.AssignRoles(commandContextContainer, registrationId.Value)
                                                                      .ConfigureAwait(false);

                                           await MessageBuilder.RefreshMessageAsync(appointment.ConfigurationId)
                                                               .ConfigureAwait(false);

                                           await commandContextContainer.Message
                                                                        .DeleteAsync()
                                                                        .ConfigureAwait(false);
                                       }
                                   }
                                   else
                                   {
                                       await commandContextContainer.Message
                                                                    .RespondAsync(LocalizationGroup.GetText("NoActiveAppointment", "Currently there is no active appointment."))
                                                                    .ConfigureAwait(false);
                                   }
                               }
                           });
    }

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="commandContext">Current command context</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("leave")]
    [RequireGuild]
    public Task Leave(CommandContext commandContext, string name)
    {
        return InvokeAsync(commandContext,
                           async commandContextContainer =>
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
                                       var user = await commandContextContainer.GetCurrentUser()
                                                                               .ConfigureAwait(false);

                                       if (await RegistrationService.Leave(appointment.Id, user.Id)
                                                                    .ConfigureAwait(false))
                                       {
                                           await MessageBuilder.RefreshMessageAsync(appointment.ConfigurationId)
                                                               .ConfigureAwait(false);
                                       }

                                       await commandContextContainer.Message
                                                                    .DeleteAsync()
                                                                    .ConfigureAwait(false);
                                   }
                                   else
                                   {
                                       await commandContextContainer.Message
                                                                    .RespondAsync(LocalizationGroup.GetText("NoActiveAppointment", "Currently there is no active appointment."))
                                                                    .ConfigureAwait(false);
                                   }
                               }
                           });
    }

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="commandContext">Current command context</param>
    /// <param name="user">User</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("leaveUser")]
    [RequireGuild]
    [RequireAdministratorPermissions]
    public Task Leave(CommandContext commandContext, DiscordUser user, string name)
    {
        return InvokeAsync(commandContext,
                           async commandContextContainer =>
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
                                       var internalUser = await _userManagementService.GetUserByDiscordAccountId(user.Id)
                                                                                      .ConfigureAwait(false);

                                       if (await RegistrationService.Leave(appointment.Id, internalUser.Id)
                                                                    .ConfigureAwait(false))
                                       {
                                           await MessageBuilder.RefreshMessageAsync(appointment.ConfigurationId)
                                                               .ConfigureAwait(false);
                                       }

                                       await commandContextContainer.Message
                                                                    .DeleteAsync()
                                                                    .ConfigureAwait(false);
                                   }
                                   else
                                   {
                                       await commandContextContainer.Message
                                                                    .RespondAsync(LocalizationGroup.GetText("NoActiveAppointment", "Currently there is no active appointment."))
                                                                    .ConfigureAwait(false);
                                   }
                               }
                           });
    }

    /// <summary>
    /// Commiting the current raid appointment
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <param name="name">Alias name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setTemplate")]
    [RequireGuild]
    [RequireAdministratorPermissions]
    public Task SetTemplate(CommandContext commandContext, string name)
    {
        return InvokeAsync(commandContext,
                           async commandContextContainer =>
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
                                       if (await RegistrationService.SetTemplate(commandContextContainer, appointment.Id)
                                                                    .ConfigureAwait(false))
                                       {
                                           await MessageBuilder.RefreshMessageAsync(appointment.ConfigurationId)
                                                               .ConfigureAwait(false);

                                           await commandContextContainer.Channel
                                                                        .SendMessageAsync(LocalizationGroup.GetText("TemplateChanged", "The template has been changed."))
                                                                        .ConfigureAwait(false);
                                       }
                                   }
                                   else
                                   {
                                       await commandContextContainer.Message
                                                                    .RespondAsync(LocalizationGroup.GetText("NoActiveAppointment", "Currently there is no active appointment."))
                                                                    .ConfigureAwait(false);
                                   }
                               }
                           });
    }

    /// <summary>
    /// Commiting the current raid appointment
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <param name="aliasName">Alias name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("commit")]
    [RequireGuild]
    [RequireAdministratorPermissions]
    public Task Commit(CommandContext commandContext, string aliasName)
    {
        return InvokeAsync(commandContext,
                           async commandContextContainer =>
                           {
                               await CommitService.CommitRaidAppointment(commandContextContainer, aliasName)
                                                  .ConfigureAwait(false);
                           });
    }

    /// <summary>
    /// Post guides overview
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("guides")]
    public async Task Guides(CommandContext commandContext)
    {
        var builder = new DiscordEmbedBuilder()
                      .WithTitle(LocalizationGroup.GetText("RaidGuides", "Raid guides"))
                      .WithColor(DiscordColor.Green)
                      .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/ef1f3e1f3f40100fb3750f8d7d25c657.png?size=64")
                      .WithTimestamp(DateTime.Now);

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848910035747864576)} {Formatter.MaskedUrl("Vale Guardian", new Uri("https://wiki.guildwars2.com/wiki/Vale_Guardian"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=qtzMDCVHlLg"))} - {Formatter.MaskedUrl("Tekkit", new Uri("https://www.youtube.com/watch?v=IiSsq85T23Q"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 743938320459104317)} {Formatter.MaskedUrl("Spirit Woods", new Uri("https://wiki.guildwars2.com/wiki/Traverse_the_Spirit_Woods"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=Ni_-HkRtmm4"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848908993538949131)} {Formatter.MaskedUrl("Gorseval the Multifarious", new Uri("https://wiki.guildwars2.com/wiki/Gorseval_the_Multifarious"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=B9qbuP9kv4Q"))} - {Formatter.MaskedUrl("Tekkit", new Uri("https://www.youtube.com/watch?v=J6SnUnsppPc"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848909543915651072)} {Formatter.MaskedUrl("Sabetha the Saboteur", new Uri("https://wiki.guildwars2.com/wiki/Sabetha_the_Saboteur"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=B9qbuP9kv4Q"))} - {Formatter.MaskedUrl("Tekkit", new Uri("https://www.youtube.com/watch?v=yFo5QPoZ1aM"))})");
        builder.AddField("Wing 1 - Spirit Vale", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848909627982610482)} {Formatter.MaskedUrl("Slothasor", new Uri("https://wiki.guildwars2.com/wiki/Slothasor"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=WIBTTAPd3ME"))} - {Formatter.MaskedUrl("Tekkit", new Uri("https://www.youtube.com/watch?v=pWO-9zdVJlc"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848909882115358720)} {Formatter.MaskedUrl("Bandit Trio", new Uri("https://wiki.guildwars2.com/wiki/Protect_the_caged_prisoners"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=ncVYseut8ag"))} - {Formatter.MaskedUrl("Tekkit", new Uri("https://www.youtube.com/watch?v=wEMMRwoM56Q"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848909162821845043)} {Formatter.MaskedUrl("Matthias Gabrel", new Uri("https://wiki.guildwars2.com/wiki/Matthias_Gabrel"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=q99ocDpSnAI"))})");
        builder.AddField("Wing 2 - Salvation Pass", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 743938372195844117)} {Formatter.MaskedUrl("Siege the Stronghold", new Uri("https://wiki.guildwars2.com/wiki/Siege_the_Stronghold"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=WFwcnj74lwU"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848909049599885322)} {Formatter.MaskedUrl("Keep Construct", new Uri("https://wiki.guildwars2.com/wiki/Keep_Construct"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=QIdzfDtxaFQ"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848909953112473622)} {Formatter.MaskedUrl("Twisted Castle", new Uri("https://wiki.guildwars2.com/wiki/Traverse_the_Twisted_Castle"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=mRwLCQ7rUTw"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848910090370940949)} {Formatter.MaskedUrl("Xera", new Uri("https://wiki.guildwars2.com/wiki/Xera"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=Iggsq-f3yBw"))})");
        builder.AddField("Wing 3 - Stronghold of the Faithful", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848908521680142359)} {Formatter.MaskedUrl("Cairn the Indomitable", new Uri("https://wiki.guildwars2.com/wiki/Cairn_the_Indomitable"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=zPfLE1wwWkw"))} - {Formatter.MaskedUrl("Tekkit", new Uri("https://www.youtube.com/watch?v=SjzG0qEi20o"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848909340827713557)} {Formatter.MaskedUrl("Mursaat Overseer", new Uri("https://wiki.guildwars2.com/wiki/Mursaat_Overseer"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=iHMzAPWoLDU"))} - {Formatter.MaskedUrl("Tekkit", new Uri("https://www.youtube.com/watch?v=pcYEEX6RgDM"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848909587938803762)} {Formatter.MaskedUrl("Samarog", new Uri("https://wiki.guildwars2.com/wiki/Samarog"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=pLudL0RBgf0"))} - {Formatter.MaskedUrl("Tekkit", new Uri("https://www.youtube.com/watch?v=diwPG9Dsrt8"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848908773996101642)} {Formatter.MaskedUrl("Deimos", new Uri("https://wiki.guildwars2.com/wiki/Deimos"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=gW5HH36X3zU"))} - {Formatter.MaskedUrl("Tekkit", new Uri("https://www.youtube.com/watch?v=04KJ9Mtocmw"))})");
        builder.AddField("Wing 4 - Bastion of the Penitent", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848911345964679188)} {Formatter.MaskedUrl("Soulless Horror", new Uri("https://wiki.guildwars2.com/wiki/Soulless_Horror"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=Nw6D9_RcJhs"))} - {Formatter.MaskedUrl("Tekkit", new Uri("https://www.youtube.com/watch?v=FquU1cnv1Xc"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 743940484455596064)} {Formatter.MaskedUrl("River of Souls", new Uri("https://wiki.guildwars2.com/wiki/Traverse_the_River_of_Souls"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=-84PZQGCmyI"))} - {Formatter.MaskedUrl("Tekkit", new Uri("https://www.youtube.com/watch?v=gCC8EjHHMzE"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848909739509547058)}{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848908876039585822)}{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848908317832773692)} {Formatter.MaskedUrl("Statues of Grenth", new Uri("https://wiki.guildwars2.com/wiki/Restore_the_three_statues_of_Grenth"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=cKUcf_ez9Ec"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848908828866379777)} {Formatter.MaskedUrl("Dhuum", new Uri("https://wiki.guildwars2.com/wiki/Dhuum"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=5gFxLKJI9ds"))} - {Formatter.MaskedUrl("Tekkit", new Uri("https://www.youtube.com/watch?v=uEfAH8DdaUg"))})");
        builder.AddField("Wing 5 - Hall of Chains", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848908712692547614)} {Formatter.MaskedUrl("Conjured Amalgamate", new Uri("https://wiki.guildwars2.com/wiki/Conjured_Amalgamate"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=B7VQAZuV-6o"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848909098619895808)} {Formatter.MaskedUrl("Twin Largos", new Uri("https://wiki.guildwars2.com/wiki/Defeat_the_twin_largos"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=RUQQhyE_YiA"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848909410691973140)} {Formatter.MaskedUrl("Qadim", new Uri("https://wiki.guildwars2.com/wiki/Qadim"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=nU_yQtk_8oo"))})");
        builder.AddField("Wing 6 - Mythwright Gambit", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848908653637533736)} {Formatter.MaskedUrl("Cardinal Sabir", new Uri("https://wiki.guildwars2.com/wiki/Cardinal_Sabir"))} ({Formatter.MaskedUrl("Mukluk", new Uri("https://www.youtube.com/watch?v=jwjqrsBQhEo"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848908580749049866)} {Formatter.MaskedUrl("Cardinal Adina", new Uri("https://wiki.guildwars2.com/wiki/Cardinal_Adina"))} ({Formatter.MaskedUrl("MightyTeapot", new Uri("https://www.youtube.com/watch?v=UY1iBmxDD6M"))})");
        stringBuilder.AppendLine($"{DiscordEmojiService.GetGuildEmoji(commandContext.Client, 848909465553207296)} {Formatter.MaskedUrl("Qadim the Peerless", new Uri("https://wiki.guildwars2.com/wiki/Qadim_the_Peerless"))} ({Formatter.MaskedUrl("Hepha Cinema", new Uri("https://www.youtube.com/watch?v=vUELGR2S2Mc"))})");
        builder.AddField("Wing 7 - The Key of Ahdashim", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.Append($"{Formatter.MaskedUrl("Xera", new Uri("https://cdn.discordapp.com/attachments/847555191842537552/888025983649722399/unknown.png"))}");
        stringBuilder.Append(" - ");
        stringBuilder.Append($"{Formatter.MaskedUrl("Dhuum", new Uri("https://cdn.discordapp.com/attachments/847555191842537552/888031307798556672/5cRRIt5NNdTcj2KUwYrzIbuVxpYTXN7Qknn9lSNhiWw.png"))}");
        stringBuilder.Append(" - ");
        stringBuilder.Append($"{Formatter.MaskedUrl("Qadim", new Uri("https://media.discordapp.net/attachments/847555191842537552/888032050534301706/MRM_FYBHoCRB9diOF_-dNZtAby8XID3Lcnp15OF151U.png?width=1052&height=1052"))}");
        stringBuilder.Append(" - ");
        stringBuilder.Append($"{Formatter.MaskedUrl("Qadim the Peerless", new Uri("https://media.discordapp.net/attachments/847555191842537552/888031828055830588/e_4wRNQcQtoxZ8Nb52T_qZlB0SI-0DhfI5JzNGXA76o.png?width=1052&height=624"))}");
        builder.AddField("Markers / Reference sheets", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{Formatter.MaskedUrl("Snow Crows", new Uri("https://snowcrows.com/"))} - {Formatter.MaskedUrl("Lucky Noobs", new Uri("https://lucky-noobs.com/"))} - {Formatter.MaskedUrl("Hardstuck", new Uri("https://hardstuck.gg/"))}");
        builder.AddField("Builds", stringBuilder.ToString());

        await commandContext.RespondAsync(builder)
                            .ConfigureAwait(false);
    }

    #endregion // Methods

    #region Roles

    /// <summary>
    /// Role administration
    /// </summary>
    [Group("roles")]
    [Aliases("r")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class RaidRolesCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="userManagementService">User management service</param>
        /// <param name="httpClientFactory">HttpClient-Factory</param>
        public RaidRolesCommandModule(LocalizationService localizationService, UserManagementService userManagementService, IHttpClientFactory httpClientFactory)
            : base(localizationService, userManagementService, httpClientFactory)
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
        [RequireGuild]
        [RequireAdministratorPermissions]
        public Task SetupRoles(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await RaidRolesService.RunAssistantAsync(commandContextContainer)
                                                         .ConfigureAwait(false);
                               });
        }

        #endregion // Methods
    }

    #endregion // Roles

    #region Templates

    /// <summary>
    /// Template administration
    /// </summary>
    [Group("templates")]
    [Aliases("t")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class RaidTemplatesCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="userManagementService">User management service</param>
        /// <param name="httpClientFactory">HttpClient-Factory</param>
        public RaidTemplatesCommandModule(LocalizationService localizationService, UserManagementService userManagementService, IHttpClientFactory httpClientFactory)
            : base(localizationService, userManagementService, httpClientFactory)
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
        [RequireGuild]
        [RequireAdministratorPermissions]
        public Task Setup(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   bool repeat;

                                   do
                                   {
                                       repeat = await DialogHandler.Run<RaidTemplateSetupDialogElement, bool>(commandContextContainer).ConfigureAwait(false);
                                   }
                                   while (repeat);
                               });
        }

        #endregion // Methods
    }

    #endregion // Templates

    #region Levels

    /// <summary>
    /// Template administration
    /// </summary>
    [Group("levels")]
    [Aliases("l", "level")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class RaidExperienceLevelsCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="userManagementService">User management service</param>
        /// <param name="httpClientFactory">HttpClient-Factory</param>
        public RaidExperienceLevelsCommandModule(LocalizationService localizationService, UserManagementService userManagementService, IHttpClientFactory httpClientFactory)
            : base(localizationService, userManagementService, httpClientFactory)
        {
        }

        #endregion // Constructor

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
        public RaidMessageBuilder MessageBuilder  { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Starting the experience levels assistant
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        public Task Setup(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   bool repeat;

                                   do
                                   {
                                       repeat = await DialogHandler.Run<RaidExperienceLevelSetupDialogElement, bool>(commandContextContainer)
                                                                   .ConfigureAwait(false);
                                   }
                                   while (repeat);
                               });
        }

        /// <summary>
        /// Set experience levels to players
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <param name="aliasName">Alias name</param>
        /// <param name="discordUsers">Users</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("set")]
        [RequireGuild]
        [RequireAdministratorPermissions]
        public Task SetExperienceLevel(CommandContext commandContext, string aliasName, params DiscordUser[] discordUsers)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
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
                                           var changedRanks = new List<(DiscordUser User, long? OldExperienceLevelId, long NewExperienceLevelId)>();

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
                                                                     }) == false)
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

                                           await commandContextContainer.Message
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

                                               await commandContextContainer.Channel
                                                                            .SendMessageAsync(LocalizationGroup.GetFormattedText("NewRankFormat",
                                                                                                                                 "Level changed for {0}: {1} {2} {3}",
                                                                                                                                 changedRank.User.Mention,
                                                                                                                                 oldRank?.Description,
                                                                                                                                 oldRank == null || oldRank.Rank > newRank?.Rank
                                                                                                                                     ? DiscordEmojiService.GetArrowUpEmoji(commandContext.Client)
                                                                                                                                     : DiscordEmojiService.GetArrowDownEmoji(commandContext.Client),
                                                                                                                                 newRank?.Description))
                                                                            .ConfigureAwait(false);
                                           }
                                       }
                                       else
                                       {
                                           await commandContextContainer.Message
                                                                        .RespondAsync(LocalizationGroup.GetText("UnknownExperienceLevel", "The experience role by the given name does not exist."))
                                                                        .ConfigureAwait(false);
                                       }
                                   }
                               });
        }

        #endregion // Methods
    }

    #endregion // Templates

    #region Levels

    /// <summary>
    /// Overviews
    /// </summary>
    [Group("overview")]
    [Aliases("o")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class RaidOverviewCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="userManagementService">User management service</param>
        /// <param name="httpClientFactory">HttpClient-Factory</param>
        public RaidOverviewCommandModule(LocalizationService localizationService, UserManagementService userManagementService, IHttpClientFactory httpClientFactory)
            : base(localizationService, userManagementService, httpClientFactory)
        {
        }

        #endregion // Constructor

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
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("participation")]
        [RequireGuild]
        public Task PostParticipationOverview(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await RaidOverviewService.PostParticipationOverview(commandContextContainer)
                                                            .ConfigureAwait(false);
                               });
        }

        /// <summary>
        /// Post overview of experience roles
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("levels")]
        [RequireGuild]
        public Task PostExperienceLevelOverview(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await RaidExperienceLevelsService.PostExperienceLevelOverview(commandContextContainer)
                                                                    .ConfigureAwait(false);
                               });
        }

        #endregion // Methods
    }

    #endregion // Templates
}
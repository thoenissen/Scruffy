using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.LookingForGroup;
using Scruffy.Data.Entity.Tables.LookingForGroup;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;
using Scruffy.Services.LookingForGroup.Modals;

namespace Scruffy.Services.LookingForGroup;

/// <summary>
/// Looking for group command handler
/// </summary>
public class LookingForGroupCommandHandler : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory;

    /// <summary>
    /// User management service
    /// </summary>
    private readonly UserManagementService _userManagementService;

    /// <summary>
    /// Message service
    /// </summary>
    private readonly LookingForGroupMessageService _messageService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="userManagementService">User management service</param>
    /// <param name="messageService">Message service</param>
    /// <param name="repositoryFactory">Repository factory</param>
    public LookingForGroupCommandHandler(LocalizationService localizationService,
                                         UserManagementService userManagementService,
                                         LookingForGroupMessageService messageService,
                                         RepositoryFactory repositoryFactory)
        : base(localizationService)
    {
        _userManagementService = userManagementService;
        _messageService = messageService;
        _repositoryFactory = repositoryFactory;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Starting the creation of an new appointment
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task StartCreation(InteractionContextContainer context)
    {
        await context.RespondWithModalAsync<LookingForGroupCreationModalData>(LookingForGroupCreationModalData.CustomId)
                     .ConfigureAwait(false);
    }

    /// <summary>
    /// Creation of a new appointment
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="title">Title</param>
    /// <param name="dateString">Time of the appointment</param>
    /// <param name="countString">Number of participants</param>
    /// <param name="description">Description</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task Create(InteractionContextContainer context, string title, string dateString, string countString, string description)
    {
        var userTask = _userManagementService.GetUserByDiscordAccountId(context.User);

        if (context.Channel is ITextChannel textChannel)
        {
            var message = await context.ReplyAsync(DiscordEmoteService.GetLoadingEmote(context.Client) + " " + LocalizationGroup.GetText("AppointmentCreation", "The appoint is being created."))
                                       .ConfigureAwait(false);

            var thread = await textChannel.CreateThreadAsync("LFG: " + title)
                                          .ConfigureAwait(false);

            await thread.AddUserAsync(context.Member)
                        .ConfigureAwait(false);

            await textChannel.DeleteMessageAsync(thread.Id)
                             .ConfigureAwait(false);

            var user = await userTask.ConfigureAwait(false);

            var appointment = new LookingForGroupAppointmentEntity
                              {
                                  ChannelId = textChannel.Id,
                                  MessageId = message.Id,
                                  CreationUserId = user.Id,
                                  Title = title,
                                  Description = description,
                                  ThreadId = thread.Id
                              };

            if (DateTime.TryParse(dateString, out var appointmentDate))
            {
                appointment.Date = appointmentDate;
            }

            if (int.TryParse(countString, out var participantCount)
                && participantCount > 0)
            {
                appointment.ParticipantCount = participantCount;
            }

            if (_repositoryFactory.GetRepository<LookingForGroupAppointmentRepository>()
                                  .Add(appointment))
            {
                _repositoryFactory.GetRepository<LookingForGroupParticipantRepository>()
                                  .Add(new LookingForGroupParticipantEntity
                                       {
                                           AppointmentId = appointment.Id,
                                           RegistrationTimeStamp = DateTime.Now,
                                           UserId = user.Id
                                       });

                await _messageService.RefreshMessage(appointment.Id)
                                     .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Edit of an appointment
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="appointmentId">Appointment id</param>
    /// <param name="title">Title</param>
    /// <param name="dateString">Time of the appointment</param>
    /// <param name="countString">Number of participants</param>
    /// <param name="description">Description</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task Edit(InteractionContextContainer context, int appointmentId, string title, string dateString, string countString, string description)
    {
        await context.DeferAsync()
                     .ConfigureAwait(false);

        if (_repositoryFactory.GetRepository<LookingForGroupAppointmentRepository>()
                              .Refresh(obj => obj.Id == appointmentId,
                                       obj =>
                                       {
                                           obj.Title = title;
                                           obj.Description = description;

                                           if (DateTime.TryParse(dateString, out var appointmentDate))
                                           {
                                               obj.Date = appointmentDate;
                                           }
                                           else
                                           {
                                               obj.Date = null;
                                           }

                                           if (int.TryParse(countString, out var participantCount)
                                               && participantCount > 0)
                                           {
                                               obj.ParticipantCount = participantCount;
                                           }
                                           else
                                           {
                                               obj.ParticipantCount = null;
                                           }
                                       }))
        {
            await _messageService.RefreshMessage(appointmentId)
                                 .ConfigureAwait(false);

            var threadId = _repositoryFactory.GetRepository<LookingForGroupAppointmentRepository>()
                                             .GetQuery()
                                             .Where(obj => obj.Id == appointmentId)
                                             .Select(obj => obj.ThreadId)
                                             .FirstOrDefault();

            if (threadId > 0)
            {
                if (await context.Client
                                 .GetChannelAsync(threadId)
                                 .ConfigureAwait(false) is IThreadChannel threadChannel)
                {
                    await threadChannel.ModifyAsync(obj => obj.Name = "LFG: " + title)
                                       .ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="appointmentId">Id of the appointment</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Join(InteractionContextContainer context, int appointmentId)
    {
        var user = await _userManagementService.GetUserByDiscordAccountId(context.User)
                                               .ConfigureAwait(false);

        if (_repositoryFactory.GetRepository<LookingForGroupParticipantRepository>()
                              .Add(new LookingForGroupParticipantEntity
                                   {
                                       AppointmentId = appointmentId,
                                       RegistrationTimeStamp = DateTime.Now,
                                       UserId = user.Id
                                   }))
        {
            await _messageService.RefreshMessage(appointmentId)
                                 .ConfigureAwait(false);

            var threadId = _repositoryFactory.GetRepository<LookingForGroupAppointmentRepository>()
                                             .GetQuery()
                                             .Where(obj => obj.Id == appointmentId)
                                             .Select(obj => obj.ThreadId)
                                             .FirstOrDefault();

            if (threadId > 0)
            {
                if (await context.Client
                                 .GetChannelAsync(threadId)
                                 .ConfigureAwait(false) is IThreadChannel threadChannel)
                {
                    await threadChannel.AddUserAsync(context.Member)
                                       .ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="appointmentId">Id of the appointment</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Leave(InteractionContextContainer context, int appointmentId)
    {
        var user = await _userManagementService.GetUserByDiscordAccountId(context.User)
                                               .ConfigureAwait(false);

        if (_repositoryFactory.GetRepository<LookingForGroupParticipantRepository>()
                              .Remove(obj => obj.AppointmentId == appointmentId
                                          && obj.UserId == user.Id))
        {
            await _messageService.RefreshMessage(appointmentId)
                                 .ConfigureAwait(false);

            var threadId = _repositoryFactory.GetRepository<LookingForGroupAppointmentRepository>()
                                             .GetQuery()
                                             .Where(obj => obj.Id == appointmentId)
                                             .Select(obj => obj.ThreadId)
                                             .FirstOrDefault();

            if (threadId > 0)
            {
                if (await context.Client
                                 .GetChannelAsync(threadId)
                                 .ConfigureAwait(false) is IThreadChannel threadChannel)
                {
                    await threadChannel.RemoveUserAsync(context.Member)
                                       .ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Configuration an appointment
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="appointmentId">Id of the appointment</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Configure(InteractionContextContainer context, int appointmentId)
    {
        if (context.User is IGuildUser { GuildPermissions.Administrator: true }
         || _repositoryFactory.GetRepository<LookingForGroupAppointmentRepository>()
                              .GetQuery()
                              .Any(obj => obj.Id == appointmentId
                                       && obj.CreationUser.DiscordAccounts.Any(obj2 => obj2.Id == context.User.Id)))
        {
            var embedBuilder = new EmbedBuilder().WithTitle(LocalizationGroup.GetText("ConfigurationTitle", "Configuration assistant"))
                                                 .WithDescription(LocalizationGroup.GetText("ConfigurationDescription", "With the following assistant you can configure the existing appoint. You can dismiss this message, if you don't want to edit anything anymore."))
                                                 .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                                 .WithColor(Color.Green)
                                                 .WithTimestamp(DateTime.Now);

            var componentsBuilder = new ComponentBuilder();

            componentsBuilder.WithSelectMenu(new SelectMenuBuilder().AddOption(LocalizationGroup.GetText("ConfigurationMenuEdit", "Edit appointment data"), "edit", emote: DiscordEmoteService.GetEditEmote(context.Client))
                                                                    .AddOption(LocalizationGroup.GetText("ConfigurationMenuDelete", "Delete appointment"), "delete", emote: DiscordEmoteService.GetTrashCanEmote(context.Client))
                                                                    .WithCustomId(InteractivityService.GetPermanentCustomId("lfg", "configureMenu", appointmentId.ToString())));

            await context.SendMessageAsync(embed: embedBuilder.Build(),
                                           components: componentsBuilder.Build(),
                                           ephemeral: true)
                         .ConfigureAwait(false);
        }
        else
        {
            await context.ReplyAsync(LocalizationGroup.GetText("ConfigurationPermissionsDenied", "You are not allowed to edit the appointment."),
                                     ephemeral: true)
                         .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Configure menu options
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="appointmentId">Id of the appointment</param>
    /// <param name="value">Selected value</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ConfigureMenuOption(InteractionContextContainer context, int appointmentId, string value)
    {
        var appointment = _repositoryFactory.GetRepository<LookingForGroupAppointmentRepository>()
                                            .GetQuery()
                                            .Where(obj => obj.Id == appointmentId)
                                            .Select(obj => new
                                                           {
                                                               obj.Title,
                                                               obj.Date,
                                                               obj.ParticipantCount,
                                                               obj.Description,
                                                               IsCreator = obj.CreationUser.DiscordAccounts.Any(obj2 => obj2.Id == context.User.Id)
                                                           })
                                            .FirstOrDefault();

        if (appointment != null
            && (context.User is IGuildUser { GuildPermissions.Administrator: true }
                || appointment.IsCreator))
        {
            switch (value)
            {
                case "edit":
                    {
                        await context.RespondWithModalAsync<LookingForGroupEditModalData>($"{LookingForGroupEditModalData.CustomId};{appointmentId}",
                                                                                         builder =>
                                                                                         {
                                                                                             builder.UpdateTextInput(nameof(LookingForGroupEditModalData.AppointmentTitle), appointment.Title);

                                                                                             if (appointment.Date != null)
                                                                                             {
                                                                                                 builder.UpdateTextInput(nameof(LookingForGroupEditModalData.AppointmentTime), appointment.Date!.Value.ToString("dd.MM.yyyy HH:mm"));
                                                                                             }

                                                                                             if (appointment.ParticipantCount != null)
                                                                                             {
                                                                                                 builder.UpdateTextInput(nameof(LookingForGroupEditModalData.AppointmentParticipantCount), appointment.ParticipantCount!.Value.ToString());
                                                                                             }

                                                                                             if (string.IsNullOrEmpty(appointment.Description) == false)
                                                                                             {
                                                                                                 builder.UpdateTextInput(nameof(LookingForGroupEditModalData.AppointmentDescription), appointment.Description);
                                                                                             }
                                                                                         })
                                     .ConfigureAwait(false);

                        await context.DeleteOriginalResponse()
                                     .ConfigureAwait(false);
                    }
                    break;

                case "delete":
                    {
                        var message = await context.DeferProcessing()
                                                   .ConfigureAwait(false);

                        var data = _repositoryFactory.GetRepository<LookingForGroupAppointmentRepository>()
                                                     .GetQuery()
                                                     .Where(obj => obj.Id == appointmentId)
                                                     .Select(obj => new
                                                     {
                                                         obj.ChannelId,
                                                         obj.MessageId,
                                                         obj.ThreadId
                                                     })
                                                     .FirstOrDefault();
                        if (data != null)
                        {
                            if (await context.Client
                                             .GetChannelAsync(data.ThreadId)
                                             .ConfigureAwait(false) is IThreadChannel threadChannel)
                            {
                                await threadChannel.DeleteAsync()
                                                   .ConfigureAwait(false);
                            }

                            if (await context.Client
                                             .GetChannelAsync(data.ChannelId)
                                             .ConfigureAwait(false) is ITextChannel textChannel)
                            {
                                await textChannel.DeleteMessageAsync(data.MessageId)
                                                 .ConfigureAwait(false);
                            }
                        }

                        await message.DeleteAsync()
                                     .ConfigureAwait(false);
                    }
                    break;
            }
        }
        else
        {
            await context.ReplyAsync(LocalizationGroup.GetText("ConfigurationPermissionsDenied", "You are not allowed to edit the appointment."),
                                     ephemeral: true)
                         .ConfigureAwait(false);
        }
    }

    #endregion // Methods
}
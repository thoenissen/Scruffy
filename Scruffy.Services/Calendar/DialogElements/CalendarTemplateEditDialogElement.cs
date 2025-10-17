using Discord;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Services.Calendar.DialogElements.Forms;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar.DialogElements;

/// <summary>
/// Editing a calendar template
/// </summary>
public class CalendarTemplateEditDialogElement : DialogEmbedReactionElementBase<bool>
{
    #region Fields

    /// <summary>
    /// Reactions
    /// </summary>
    private List<ReactionData<bool>> _reactions;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public CalendarTemplateEditDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogReactionElementBase<bool>

    /// <inheritdoc/>
    public override async Task EditMessage(EmbedBuilder builder)
    {
        builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Calendar template configuration"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure the calendar template"));

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var templateId = DialogContext.GetValue<long>("CalendarTemplateId");

            var data = await dbFactory.GetRepository<CalendarAppointmentTemplateRepository>()
                                      .GetQuery()
                                      .Where(obj => obj.Id == templateId)
                                      .Select(obj => new
                                                     {
                                                         obj.Description,
                                                         obj.ReminderTime,
                                                         obj.ReminderMessage,
                                                         obj.GuildPoints
                                                     })
                                      .FirstAsync()
                                      .ConfigureAwait(false);

            builder.AddField(LocalizationGroup.GetText("Description", "Description"), data.Description);

            if (data.ReminderTime != null)
            {
                builder.AddField(LocalizationGroup.GetText("ReminderTime", "Reminder time"), data.ReminderTime.Value.ToString("hh\\:mm\\:ss"));
            }

            if (data.ReminderMessage != null)
            {
                builder.AddField(LocalizationGroup.GetText("ReminderMessage", "Reminder message"), data.ReminderMessage);
            }

            if (data.GuildPoints != null)
            {
                builder.AddField(LocalizationGroup.GetText("GuildPoints", "Points"), data.GuildPoints.Value.ToString());
            }
        }
    }

    /// <inheritdoc/>
    protected override string GetCommandTitle()
    {
        return LocalizationGroup.GetText("CommandTitle", "Commands");
    }

    /// <inheritdoc/>
    public override IReadOnlyList<ReactionData<bool>> GetReactions()
    {
        return _reactions ??= [
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetEditEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditDescriptionCommand", "{0} Edit description", DiscordEmoteService.GetEditEmote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var description = await RunSubElement<CalendarTemplateDescriptionDialogElement, string>()
                                                                       .ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var templateId = DialogContext.GetValue<long>("CalendarTemplateId");

                                                     dbFactory.GetRepository<CalendarAppointmentTemplateRepository>()
                                                              .Refresh(obj => obj.Id == templateId, obj => obj.Description = description);
                                                 }

                                                 return true;
                                             }
                                  },
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetEdit2Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditUriCommand", "{0} Edit link", DiscordEmoteService.GetEdit2Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var uri = await RunSubElement<CalendarTemplateUriDialogElement, string>()
                                                               .ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var templateId = DialogContext.GetValue<long>("CalendarTemplateId");

                                                     dbFactory.GetRepository<CalendarAppointmentTemplateRepository>()
                                                              .Refresh(obj => obj.Id == templateId, obj => obj.Uri = uri);
                                                 }

                                                 return true;
                                             }
                                  },
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetEdit3Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditAppointmentTimeCommand", "{0} Edit appointment time", DiscordEmoteService.GetEdit3Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var time = await RunSubElement<CalendarTemplateAppointmentTimeDialogElement, TimeSpan>()
                                                                .ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var templateId = DialogContext.GetValue<long>("CalendarTemplateId");

                                                     dbFactory.GetRepository<CalendarAppointmentTemplateRepository>()
                                                              .Refresh(obj => obj.Id == templateId, obj => obj.AppointmentTime = time);
                                                 }

                                                 return true;
                                             }
                                  },
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetEdit4Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditReminderCommand", "{0} Edit reminder", DiscordEmoteService.GetEdit4Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var reminder = await RunSubElement<CalendarTemplateReminderDialogElement, CalenderTemplateReminderData>().ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var templateId = DialogContext.GetValue<long>("CalendarTemplateId");

                                                     dbFactory.GetRepository<CalendarAppointmentTemplateRepository>()
                                                              .Refresh(obj => obj.Id == templateId,
                                                                       obj =>
                                                                       {
                                                                           obj.ReminderTime = reminder?.Time;
                                                                           obj.ReminderMessage = reminder?.Message;
                                                                       });
                                                 }

                                                 return true;
                                             }
                                  },
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetEdit5Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditGuildPointsCommand", "{0} Edit guild points", DiscordEmoteService.GetEdit5Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var guildPoints = await RunSubElement<CalendarTemplateGuildPointsDialogElement, CalenderTemplateGuildData>().ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var templateId = DialogContext.GetValue<long>("CalendarTemplateId");

                                                     dbFactory.GetRepository<CalendarAppointmentTemplateRepository>()
                                                              .Refresh(obj => obj.Id == templateId,
                                                                       obj =>
                                                                       {
                                                                           obj.GuildPoints = guildPoints?.Points;
                                                                           obj.IsRaisingGuildPointCap = guildPoints?.IsRaisingPointCap;
                                                                       });
                                                 }

                                                 return true;
                                             }
                                  },
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("CancelCommand", "{0} Cancel", DiscordEmoteService.GetCrossEmote(CommandContext.Client)),
                                      Func = () => Task.FromResult(false)
                                  }
                              ];
    }

    /// <inheritdoc/>
    protected override bool DefaultFunc()
    {
        return false;
    }

    #endregion // DialogReactionElementBase<bool>
}
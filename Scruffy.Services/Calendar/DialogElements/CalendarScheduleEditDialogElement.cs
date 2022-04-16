﻿using Discord;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Data.Services.Calendar;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar.DialogElements;

/// <summary>
/// Editing a calendar schedule
/// </summary>
public class CalendarScheduleEditDialogElement : DialogEmbedReactionElementBase<bool>
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
    public CalendarScheduleEditDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogReactionElementBase<bool>

    /// <summary>
    /// Editing the embedded message
    /// </summary>
    /// <param name="builder">Builder</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task EditMessage(EmbedBuilder builder)
    {
        builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Calendar schedule configuration"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure the calendar schedule"));

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var scheduleId = DialogContext.GetValue<long>("CalendarScheduleId");

            var data = await dbFactory.GetRepository<CalendarAppointmentScheduleRepository>()
                                      .GetQuery()
                                      .Where(obj => obj.Id == scheduleId)
                                      .Select(obj => new
                                                     {
                                                         obj.Description
                                                     })
                                      .FirstAsync()
                                      .ConfigureAwait(false);

            builder.AddField(LocalizationGroup.GetText("Description", "Description"), data.Description);
        }
    }

    /// <summary>
    /// Returns the title of the commands
    /// </summary>
    /// <returns>Commands</returns>
    protected override string GetCommandTitle()
    {
        return LocalizationGroup.GetText("CommandTitle", "Commands");
    }

    /// <summary>
    /// Returns the reactions which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public override IReadOnlyList<ReactionData<bool>> GetReactions()
    {
        return _reactions ??= new List<ReactionData<bool>>
                              {
                                  new()
                                  {
                                      Emote = DiscordEmoteService.GetEditEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditDescriptionCommand", "{0} Edit description", DiscordEmoteService.GetEditEmote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var description = await RunSubElement<CalendarScheduleDescriptionDialogElement, string>()
                                                                       .ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var scheduleId = DialogContext.GetValue<long>("CalendarScheduleId");

                                                     dbFactory.GetRepository<CalendarAppointmentScheduleRepository>()
                                                              .Refresh(obj => obj.Id == scheduleId, obj => obj.Description = description);
                                                 }

                                                 return true;
                                             }
                                  },
                                  new()
                                  {
                                      Emote = DiscordEmoteService.GetEdit2Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditScheduleCommand", "{0} Edit schedule", DiscordEmoteService.GetEdit2Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var data = await RunSubElement<CalendarScheduleScheduleDialogElement, CalenderScheduleData>()
                                                                .ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var scheduleId = DialogContext.GetValue<long>("CalendarScheduleId");

                                                     dbFactory.GetRepository<CalendarAppointmentScheduleRepository>()
                                                              .Refresh(obj => obj.Id == scheduleId, obj =>
                                                                                                    {
                                                                                                        obj.Type = data.Type;
                                                                                                        obj.AdditionalData = data.AdditionalData;
                                                                                                    });
                                                 }

                                                 return true;
                                             }
                                  },
                                  new()
                                  {
                                      Emote = DiscordEmoteService.GetEdit3Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditTemplateCommand", "{0} Edit template", DiscordEmoteService.GetEdit3Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var templateId = await RunSubElement<CalendarTemplateSelectionDialogElement, long>()
                                                                      .ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var scheduleId = DialogContext.GetValue<long>("CalendarScheduleId");

                                                     dbFactory.GetRepository<CalendarAppointmentScheduleRepository>()
                                                              .Refresh(obj => obj.Id == scheduleId, obj => obj.CalendarAppointmentTemplateId = templateId);
                                                 }

                                                 return true;
                                             }
                                  },
                                  new()
                                  {
                                      Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("CancelCommand", "{0} Cancel", DiscordEmoteService.GetCrossEmote(CommandContext.Client)),
                                      Func = () => Task.FromResult(false)
                                  }
                              };
    }

    /// <summary>
    /// Default case if none of the given reactions is used
    /// </summary>
    /// <returns>Result</returns>
    protected override bool DefaultFunc()
    {
        return false;
    }

    #endregion DialogReactionElementBase<bool>
}
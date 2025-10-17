using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Data.Entity.Tables.Calendar;
using Scruffy.Services.Calendar.DialogElements.Forms;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar.DialogElements;

/// <summary>
/// Starting the calendar schedule assistant
/// </summary>
public class CalendarScheduleSetupDialogElement : DialogEmbedReactionElementBase<bool>
{
    #region Fields

    /// <summary>
    /// Reactions
    /// </summary>
    private List<ReactionData<bool>> _reactions;

    /// <summary>
    /// Schedules
    /// </summary>
    private List<string> _schedules;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public CalendarScheduleSetupDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Returns the existing schedules
    /// </summary>
    /// <returns>Levels</returns>
    private List<string> GetSchedules()
    {
        if (_schedules == null)
        {
            var serverId = CommandContext.Guild.Id;

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                _schedules = dbFactory.GetRepository<CalendarAppointmentScheduleRepository>()
                                      .GetQuery()
                                      .Where(obj => obj.DiscordServerId == serverId)
                                      .Select(obj => obj.Description)
                                      .ToList();
            }
        }

        return _schedules;
    }

    #endregion // Methods

    #region DialogReactionElementBase<bool>

    /// <inheritdoc/>
    public override Task EditMessage(EmbedBuilder builder)
    {
        builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Calendar schedule configuration"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure the calendar schedules. The following schedules are already created:"));

        var schedulesBuilder = new StringBuilder();

        var schedules = GetSchedules();

        if (schedules.Count > 0)
        {
            foreach (var schedule in schedules)
            {
                schedulesBuilder.AppendLine(schedule);
            }
        }
        else
        {
            schedulesBuilder.Append('\u200B');
        }

        builder.AddField(LocalizationGroup.GetText("SchedulesField", "Schedules"), schedulesBuilder.ToString());

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override IReadOnlyList<ReactionData<bool>> GetReactions()
    {
        if (_reactions == null)
        {
            _reactions = [
                             new ReactionData<bool>
                             {
                                 Emote = DiscordEmoteService.GetAddEmote(CommandContext.Client),
                                 CommandText = LocalizationGroup.GetFormattedText("AddCommand", "{0} Add schedule", DiscordEmoteService.GetAddEmote(CommandContext.Client)),
                                 Func = async () =>
                                        {
                                            var data = await DialogHandler.RunForm<CreateCalendarScheduleData>(CommandContext, false)
                                                                          .ConfigureAwait(false);

                                            using (var dbFactory = RepositoryFactory.CreateInstance())
                                            {
                                                var level = new CalendarAppointmentScheduleEntity
                                                            {
                                                                DiscordServerId = CommandContext.Guild.Id,
                                                                Description = data.Description,
                                                                CalendarAppointmentTemplateId = data.TemplateId,
                                                                Type = data.Schedule.Type,
                                                                AdditionalData = data.Schedule.AdditionalData
                                                            };

                                                dbFactory.GetRepository<CalendarAppointmentScheduleRepository>()
                                                         .Add(level);
                                            }

                                            return true;
                                        }
                             }
                          ];

            if (GetSchedules().Count > 0)
            {
                _reactions.Add(new ReactionData<bool>
                               {
                                   Emote = DiscordEmoteService.GetEditEmote(CommandContext.Client),
                                   CommandText = LocalizationGroup.GetFormattedText("EditCommand", "{0} Edit schedule", DiscordEmoteService.GetEditEmote(CommandContext.Client)),
                                   Func = async () =>
                                          {
                                              var levelId = await RunSubElement<CalendarScheduleSelectionDialogElement, long>().ConfigureAwait(false);

                                              DialogContext.SetValue("CalendarScheduleId", levelId);

                                              bool repeat;

                                              do
                                              {
                                                  repeat = await RunSubElement<CalendarScheduleEditDialogElement, bool>().ConfigureAwait(false);
                                              }
                                              while (repeat);

                                              return true;
                                          }
                               });

                _reactions.Add(new ReactionData<bool>
                               {
                                   Emote = DiscordEmoteService.GetTrashCanEmote(CommandContext.Client),
                                   CommandText = LocalizationGroup.GetFormattedText("DeleteCommand", "{0} Delete schedule", DiscordEmoteService.GetTrashCanEmote(CommandContext.Client)),
                                   Func = async () =>
                                          {
                                              var levelId = await RunSubElement<CalendarScheduleSelectionDialogElement, long>().ConfigureAwait(false);

                                              DialogContext.SetValue("CalendarScheduleId", levelId);

                                              return await RunSubElement<CalendarScheduleDeletionDialogElement, bool>().ConfigureAwait(false);
                                          }
                               });
            }

            _reactions.Add(new ReactionData<bool>
                           {
                               Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                               CommandText = LocalizationGroup.GetFormattedText("CancelCommand", "{0} Cancel", DiscordEmoteService.GetCrossEmote(CommandContext.Client)),
                               Func = () => Task.FromResult(false)
                           });
        }

        return _reactions;
    }

    /// <inheritdoc/>
    protected override string GetCommandTitle() => LocalizationGroup.GetText("CommandTitle", "Commands");

    /// <inheritdoc/>
    protected override bool DefaultFunc()
    {
        return false;
    }

    #endregion // DialogReactionElementBase<bool>
}
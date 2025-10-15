using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Data.Entity.Tables.Calendar;
using Scruffy.Services.Calendar.DialogElements.Forms;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar.DialogElements;

/// <summary>
/// Starting the calendar template assistant
/// </summary>
public class CalendarTemplateSetupDialogElement : DialogEmbedReactionElementBase<bool>
{
    #region Fields

    /// <summary>
    /// Reactions
    /// </summary>
    private List<ReactionData<bool>> _reactions;

    /// <summary>
    /// Templates
    /// </summary>
    private List<string> _templates;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public CalendarTemplateSetupDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Returns the existing templates
    /// </summary>
    /// <returns>Levels</returns>
    private List<string> GetTemplates()
    {
        if (_templates == null)
        {
            var serverId = CommandContext.Guild.Id;

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                _templates = dbFactory.GetRepository<CalendarAppointmentTemplateRepository>()
                                      .GetQuery()
                                      .Where(obj => obj.DiscordServerId == serverId
                                                 && obj.IsDeleted == false)
                                      .Select(obj => obj.Description)
                                      .ToList();
            }
        }

        return _templates;
    }

    #endregion // Methods

    #region DialogReactionElementBase<bool>

    /// <inheritdoc/>
    public override Task EditMessage(EmbedBuilder builder)
    {
        builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Calendar template configuration"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure the calendar templates. The following templates are already created:"));

        var templatesBuilder = new StringBuilder();

        var templates = GetTemplates();

        if (templates.Count > 0)
        {
            foreach (var template in templates)
            {
                templatesBuilder.AppendLine(template);
            }
        }
        else
        {
            templatesBuilder.Append('\u200B');
        }

        builder.AddField(LocalizationGroup.GetText("TemplatesField", "Templates"), templatesBuilder.ToString());

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override IReadOnlyList<ReactionData<bool>> GetReactions()
    {
        if (_reactions == null)
        {
            _reactions = new List<ReactionData<bool>>
                         {
                             new()
                             {
                                 Emote = DiscordEmoteService.GetAddEmote(CommandContext.Client),
                                 CommandText = LocalizationGroup.GetFormattedText("AddCommand", "{0} Add template", DiscordEmoteService.GetAddEmote(CommandContext.Client)),
                                 Func = async () =>
                                        {
                                            var data = await DialogHandler.RunForm<CreateCalendarTemplateData>(CommandContext, false)
                                                                          .ConfigureAwait(false);

                                            using (var dbFactory = RepositoryFactory.CreateInstance())
                                            {
                                                var level = new CalendarAppointmentTemplateEntity
                                                            {
                                                                DiscordServerId = CommandContext.Guild.Id,
                                                                Description = data.Description,
                                                                AppointmentTime = data.AppointmentTime,
                                                                Uri = data.Uri,
                                                                ReminderMessage = data.Reminder?.Message,
                                                                ReminderTime = data.Reminder?.Time,
                                                                GuildPoints = data.GuildPoints?.Points,
                                                                IsRaisingGuildPointCap = data.GuildPoints?.IsRaisingPointCap
                                                            };

                                                dbFactory.GetRepository<CalendarAppointmentTemplateRepository>()
                                                         .Add(level);
                                            }

                                            return true;
                                        }
                             }
                         };

            if (GetTemplates().Count > 0)
            {
                _reactions.Add(new ReactionData<bool>
                               {
                                   Emote = DiscordEmoteService.GetEditEmote(CommandContext.Client),
                                   CommandText = LocalizationGroup.GetFormattedText("EditCommand", "{0} Edit template", DiscordEmoteService.GetEditEmote(CommandContext.Client)),
                                   Func = async () =>
                                          {
                                              var levelId = await RunSubElement<CalendarTemplateSelectionDialogElement, long>().ConfigureAwait(false);

                                              DialogContext.SetValue("CalendarTemplateId", levelId);

                                              bool repeat;

                                              do
                                              {
                                                  repeat = await RunSubElement<CalendarTemplateEditDialogElement, bool>().ConfigureAwait(false);
                                              }
                                              while (repeat);

                                              return true;
                                          }
                               });

                _reactions.Add(new ReactionData<bool>
                               {
                                   Emote = DiscordEmoteService.GetTrashCanEmote(CommandContext.Client),
                                   CommandText = LocalizationGroup.GetFormattedText("DeleteCommand", "{0} Delete template", DiscordEmoteService.GetTrashCanEmote(CommandContext.Client)),
                                   Func = async () =>
                                          {
                                              var levelId = await RunSubElement<CalendarTemplateSelectionDialogElement, long>().ConfigureAwait(false);

                                              DialogContext.SetValue("CalendarTemplateId", levelId);

                                              return await RunSubElement<CalendarTemplateDeletionDialogElement, bool>().ConfigureAwait(false);
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
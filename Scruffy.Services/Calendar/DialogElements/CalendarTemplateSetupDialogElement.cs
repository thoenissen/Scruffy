using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Data.Entity.Tables.Calendar;
using Scruffy.Services.Calendar.DialogElements.Forms;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Calendar.DialogElements
{
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

        /// <summary>
        /// Editing the embedded message
        /// </summary>
        /// <param name="builder">Builder</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override Task EditMessage(DiscordEmbedBuilder builder)
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

        /// <summary>
        /// Returns the reactions which should be added to the message
        /// </summary>
        /// <returns>Reactions</returns>
        public override IReadOnlyList<ReactionData<bool>> GetReactions()
        {
            if (_reactions == null)
            {
                _reactions = new List<ReactionData<bool>>
                             {
                                 new ()
                                 {
                                     Emoji = DiscordEmojiService.GetAddEmoji(CommandContext.Client),
                                     CommandText = LocalizationGroup.GetFormattedText("AddCommand", "{0} Add template", DiscordEmojiService.GetAddEmoji(CommandContext.Client)),
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
                                             Emoji = DiscordEmojiService.GetEditEmoji(CommandContext.Client),
                                             CommandText = LocalizationGroup.GetFormattedText("EditCommand", "{0} Edit template", DiscordEmojiService.GetEditEmoji(CommandContext.Client)),
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
                                             Emoji = DiscordEmojiService.GetTrashCanEmoji(CommandContext.Client),
                                             CommandText = LocalizationGroup.GetFormattedText("DeleteCommand", "{0} Delete template", DiscordEmojiService.GetTrashCanEmoji(CommandContext.Client)),
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
                                         Emoji = DiscordEmojiService.GetCrossEmoji(CommandContext.Client),
                                         CommandText = LocalizationGroup.GetFormattedText("CancelCommand", "{0} Cancel", DiscordEmojiService.GetCrossEmoji(CommandContext.Client)),
                                         Func = () => Task.FromResult(false)
                                     });
            }

            return _reactions;
        }

        /// <summary>
        /// Returns the title of the commands
        /// </summary>
        /// <returns>Commands</returns>
        protected override string GetCommandTitle() => LocalizationGroup.GetText("CommandTitle", "Commands");

        /// <summary>
        /// Default case if none of the given reactions is used
        /// </summary>
        /// <returns>Result</returns>
        protected override bool DefaultFunc()
        {
            return false;
        }

        #endregion // DialogReactionElementBase<bool>
    }
}

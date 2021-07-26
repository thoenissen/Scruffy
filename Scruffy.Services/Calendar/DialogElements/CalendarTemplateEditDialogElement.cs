using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Services.Calendar.DialogElements.Forms;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;

namespace Scruffy.Services.Calendar.DialogElements
{
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

        /// <summary>
        /// Editing the embedded message
        /// </summary>
        /// <param name="builder">Builder</param>
        public override void EditMessage(DiscordEmbedBuilder builder)
        {
            builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Calendar template configuration"));
            builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure the calendar template"));

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var templateId = DialogContext.GetValue<long>("CalendarTemplateId");

                var data = dbFactory.GetRepository<CalendarAppointmentTemplateRepository>()
                                    .GetQuery()
                                    .Where(obj => obj.Id == templateId)
                                    .Select(obj => new
                                    {
                                        obj.Description,
                                        obj.ReminderTime,
                                        obj.ReminderMessage,
                                        obj.GuildPoints
                                    })
                                    .First();

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
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetEditEmoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("EditDescriptionCommand", "{0} Edit description", DiscordEmojiService.GetEditEmoji(CommandContext.Client)),
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
                                          Emoji = DiscordEmojiService.GetEdit2Emoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("EditUriCommand", "{0} Edit link", DiscordEmojiService.GetEdit2Emoji(CommandContext.Client)),
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
                                          Emoji = DiscordEmojiService.GetEdit3Emoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("EditReminderCommand", "{0} Edit reminder", DiscordEmojiService.GetEdit3Emoji(CommandContext.Client)),
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
                                          Emoji = DiscordEmojiService.GetEdit4Emoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("EditGuildPointsCommand", "{0} Edit guild points", DiscordEmojiService.GetEdit4Emoji(CommandContext.Client)),
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
                                          Emoji = DiscordEmojiService.GetCrossEmoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("CancelCommand", "{0} Cancel", DiscordEmojiService.GetCrossEmoji(CommandContext.Client)),
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
}
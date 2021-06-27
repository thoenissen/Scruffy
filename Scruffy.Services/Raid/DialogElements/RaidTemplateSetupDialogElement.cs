﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Raid.DialogElements.Forms;

namespace Scruffy.Services.Raid.DialogElements
{
    /// <summary>
    /// Starting the raid template assistant
    /// </summary>
    public class RaidTemplateSetupDialogElement : DialogEmbedReactionElementBase<bool>
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
        public RaidTemplateSetupDialogElement(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Returns the existing templates
        /// </summary>
        /// <returns>Templates</returns>
        private List<string> GetTemplates()
        {
            if (_templates == null)
            {
                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    _templates = dbFactory.GetRepository<RaidDayTemplateRepository>()
                                          .GetQuery()
                                          .Where(obj => obj.IsDeleted == false)
                                          .Select(obj => obj.AliasName)
                                          .OrderBy(obj => obj)
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
        public override void EditMessage(DiscordEmbedBuilder builder)
        {
            builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Raid template configuration"));
            builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure the raid templates. The following templates are already created:"));

            var templatesBuilder = new StringBuilder();

            var templates = GetTemplates();
            if (templates.Count > 0)
            {
                foreach (var template in templates)
                {
                    templatesBuilder.AppendLine(Formatter.Bold($"{DiscordEmojiService.GetBulletEmoji(CommandContext.Client)} {template}"));
                }
            }
            else
            {
                templatesBuilder.Append('\u200B');
            }

            builder.AddField(LocalizationGroup.GetText("TemplatesField", "Templates"), templatesBuilder.ToString());
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
                                 new ReactionData<bool>
                                 {
                                     Emoji = DiscordEmojiService.GetAddEmoji(CommandContext.Client),
                                     CommandText = LocalizationGroup.GetFormattedText("AddCommand", "{0} Add template", DiscordEmojiService.GetAddEmoji(CommandContext.Client)),
                                     Func = async () =>
                                            {
                                                var data = await DialogHandler.RunForm<CreateRaidTemplateFormData>(CommandContext, false)
                                                                              .ConfigureAwait(false);

                                                using (var dbFactory = RepositoryFactory.CreateInstance())
                                                {
                                                    dbFactory.GetRepository<RaidDayTemplateRepository>()
                                                             .Add(new RaidDayTemplateEntity
                                                                  {
                                                                      AliasName = data.AliasName,
                                                                      Title = data.Title,
                                                                      Description = data.Description,
                                                                      Thumbnail = data.Thumbnail
                                                                  });
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
                                                  var templateId = await RunSubElement<RaidTemplateSelectionDialogElement, long>().ConfigureAwait(false);

                                                  DialogContext.SetValue("TemplateId", templateId);

                                                  bool repeat;

                                                  do
                                                  {
                                                      repeat = await RunSubElement<RaidTemplateEditDialogElement, bool>().ConfigureAwait(false);
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
                                                  var templateId = await RunSubElement<RaidTemplateSelectionDialogElement, long>().ConfigureAwait(false);

                                                  DialogContext.SetValue("TemplateId", templateId);

                                                  return await RunSubElement<RaidTemplateDeletionElementBase, bool>().ConfigureAwait(false);
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

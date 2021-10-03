using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.EventArgs;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Raid.DialogElements
{
    /// <summary>
    /// Deletion of a template
    /// </summary>
    public class RaidTemplateDeletionElementBase : DialogReactionElementBase<bool>
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
        public RaidTemplateDeletionElementBase(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region DialogReactionElementBase<bool>

        /// <summary>
        /// Editing the embedded message
        /// </summary>
        /// <returns>Message</returns>
        public override string GetMessage() => LocalizationGroup.GetText("DeleteRolePrompt", "Are you sure you want to delete the template?");

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
                                          Emoji = DiscordEmojiService.GetCheckEmoji(CommandContext.Client),
                                          Func = () =>
                                                 {
                                                     using (var dbFactory = RepositoryFactory.CreateInstance())
                                                     {
                                                         var templateId = DialogContext.GetValue<long>("TemplateId");

                                                         dbFactory.GetRepository<RaidDayTemplateRepository>()
                                                                  .Refresh(obj => obj.Id == templateId, obj => obj.IsDeleted = true);
                                                     }

                                                     return Task.FromResult(true);
                                                 }
                                      },
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetCrossEmoji(CommandContext.Client),
                                          Func = () => Task.FromResult(true)
                                      }
                                  };
        }

        /// <summary>
        /// Default case if none of the given reactions is used
        /// </summary>
        /// <param name="reaction">Reaction</param>
        /// <returns>Result</returns>
        protected override bool DefaultFunc(MessageReactionAddEventArgs reaction) => false;

        #endregion // DialogReactionElementBase<bool>
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.EventArgs;

using Scruffy.Services.Calendar.DialogElements.Forms;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;

namespace Scruffy.Services.Calendar.DialogElements
{
    /// <summary>
    /// Guild points
    /// </summary>
    public class CalendarTemplateGuildPointsDialogElement : DialogReactionElementBase<CalenderTemplateGuildData>
    {
        #region Fields

        /// <summary>
        /// Reactions
        /// </summary>
        private List<ReactionData<CalenderTemplateGuildData>> _reactions;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public CalendarTemplateGuildPointsDialogElement(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region DialogReactionElementBase<bool>

        /// <summary>
        /// Editing the embedded message
        /// </summary>
        /// <returns>Message</returns>
        public override string GetMessage() => LocalizationGroup.GetText("Prompt", "Do you want to add guild points to this event?");

        /// <summary>
        /// Returns the reactions which should be added to the message
        /// </summary>
        /// <returns>Reactions</returns>
        public override IReadOnlyList<ReactionData<CalenderTemplateGuildData>> GetReactions()
        {
            return _reactions ??= new List<ReactionData<CalenderTemplateGuildData>>
                                  {
                                      new ReactionData<CalenderTemplateGuildData>
                                      {
                                          Emoji = DiscordEmojiService.GetCheckEmoji(CommandContext.Client),
                                          Func = RunSubForm<CalenderTemplateGuildData>,
                                      },
                                      new ReactionData<CalenderTemplateGuildData>
                                      {
                                          Emoji = DiscordEmojiService.GetCrossEmoji(CommandContext.Client),
                                          Func = () => Task.FromResult<CalenderTemplateGuildData>(null)
                                      }
                                  };
        }

        /// <summary>
        /// Default case if none of the given reactions is used
        /// </summary>
        /// <param name="reaction">Reaction</param>
        /// <returns>Result</returns>
        protected override CalenderTemplateGuildData DefaultFunc(MessageReactionAddEventArgs reaction) => null;

        #endregion // DialogReactionElementBase<bool>
    }
}

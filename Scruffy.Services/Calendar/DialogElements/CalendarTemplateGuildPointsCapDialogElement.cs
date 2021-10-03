using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.EventArgs;

using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Calendar.DialogElements
{
    /// <summary>
    /// Is the guild points cap raised by this event?
    /// </summary>
    public class CalendarTemplateGuildPointsCapDialogElement : DialogReactionElementBase<bool>
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
        public CalendarTemplateGuildPointsCapDialogElement(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region DialogReactionElementBase<bool>

        /// <summary>
        /// Editing the embedded message
        /// </summary>
        /// <returns>Message</returns>
        public override string GetMessage() => LocalizationGroup.GetText("Prompt", "Is the guild points cap raised by this event?");

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
                                          Func = () => Task.FromResult(true)
                                      },
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetCrossEmoji(CommandContext.Client),
                                          Func = () => Task.FromResult(false)
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

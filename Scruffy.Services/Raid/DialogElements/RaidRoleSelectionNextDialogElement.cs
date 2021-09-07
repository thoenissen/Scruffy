using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.EventArgs;

using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;

namespace Scruffy.Services.Raid.DialogElements
{
    /// <summary>
    /// Add a role preference?
    /// </summary>
    public class RaidRoleSelectionNextDialogElement : DialogReactionElementBase<bool>
    {
        #region Fields

        /// <summary>
        /// Reactions
        /// </summary>
        private List<ReactionData<bool>> _reactions;

        /// <summary>
        /// First call?
        /// </summary>
        private bool _first;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="first">First call?</param>
        public RaidRoleSelectionNextDialogElement(LocalizationService localizationService, bool first)
            : base(localizationService)
        {
            _first = first;
        }

        #endregion // Constructor

        #region DialogReactionElementBase<bool>

        /// <summary>
        /// Editing the embedded message
        /// </summary>
        /// <returns>Message</returns>
        public override string GetMessage() => _first ? LocalizationGroup.GetText("MessageFirst", "Do you want to add role preference?")
                                                   : LocalizationGroup.GetText("MessageNext", "Do you want to add another role preference?");

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
                                      },
                                  };
        }

        /// <summary>
        /// Default case if none of the given reactions is used
        /// </summary>
        /// <param name="reaction">Reaction</param>
        /// <returns>Result</returns>
        protected override bool DefaultFunc(MessageReactionAddEventArgs reaction) => false;

        #endregion // DialogReactionElementBase
    }
}
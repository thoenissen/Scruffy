using DSharpPlus.EventArgs;

using Scruffy.Services.Calendar.DialogElements.Forms;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Calendar.DialogElements;

/// <summary>
/// Guild points
/// </summary>
public class CalendarTemplateReminderDialogElement : DialogReactionElementBase<CalenderTemplateReminderData>
{
    #region Fields

    /// <summary>
    /// Reactions
    /// </summary>
    private List<ReactionData<CalenderTemplateReminderData>> _reactions;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public CalendarTemplateReminderDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogReactionElementBase<bool>

    /// <summary>
    /// Editing the embedded message
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => LocalizationGroup.GetText("Prompt", "Do you want to add a reminder?");

    /// <summary>
    /// Returns the reactions which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public override IReadOnlyList<ReactionData<CalenderTemplateReminderData>> GetReactions()
    {
        return _reactions ??= new List<ReactionData<CalenderTemplateReminderData>>
                              {
                                  new ()
                                  {
                                      Emoji = DiscordEmojiService.GetCheckEmoji(CommandContext.Client),
                                      Func = RunSubForm<CalenderTemplateReminderData>,
                                  },
                                  new ()
                                  {
                                      Emoji = DiscordEmojiService.GetCrossEmoji(CommandContext.Client),
                                      Func = () => Task.FromResult<CalenderTemplateReminderData>(null)
                                  }
                              };
    }

    /// <summary>
    /// Default case if none of the given reactions is used
    /// </summary>
    /// <param name="reaction">Reaction</param>
    /// <returns>Result</returns>
    protected override CalenderTemplateReminderData DefaultFunc(MessageReactionAddEventArgs reaction) => null;

    #endregion // DialogReactionElementBase<bool>
}
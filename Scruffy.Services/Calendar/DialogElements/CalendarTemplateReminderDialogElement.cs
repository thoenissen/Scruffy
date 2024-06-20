using Discord;

using Scruffy.Services.Calendar.DialogElements.Forms;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

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

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("Prompt", "Do you want to add a reminder?");

    /// <inheritdoc/>
    public override IReadOnlyList<ReactionData<CalenderTemplateReminderData>> GetReactions()
    {
        return _reactions ??= new List<ReactionData<CalenderTemplateReminderData>>
                              {
                                  new()
                                  {
                                      Emote = DiscordEmoteService.GetCheckEmote(CommandContext.Client),
                                      Func = RunSubForm<CalenderTemplateReminderData>,
                                  },
                                  new()
                                  {
                                      Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                                      Func = () => Task.FromResult<CalenderTemplateReminderData>(null)
                                  }
                              };
    }

    /// <inheritdoc/>
    protected override CalenderTemplateReminderData DefaultFunc(IReaction reaction) => null;

    #endregion // DialogReactionElementBase<bool>
}
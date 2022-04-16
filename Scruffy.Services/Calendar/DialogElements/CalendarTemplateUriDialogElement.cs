using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar.DialogElements;

/// <summary>
/// Acquisition of the experience level description
/// </summary>
public class CalendarTemplateUriDialogElement : DialogReactionElementBase<string>
{
    #region Fields

    /// <summary>
    /// Reactions
    /// </summary>
    private List<ReactionData<string>> _reactions;

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public CalendarTemplateUriDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogReactionElementBase<bool>

    /// <summary>
    /// Editing the embedded message
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => LocalizationGroup.GetText("Prompt", "Do you want to add link?");

    /// <summary>
    /// Returns the reactions which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public override IReadOnlyList<ReactionData<string>> GetReactions()
    {
        return _reactions ??= new List<ReactionData<string>>
                              {
                                  new()
                                  {
                                      Emote = DiscordEmoteService.GetCheckEmote(CommandContext.Client),
                                      Func = RunSubElement<CalendarTemplateUriUriDialogElement, string>
                                  },
                                  new()
                                  {
                                      Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                                      Func = () => Task.FromResult<string>(null)
                                  }
                              };
    }

    /// <summary>
    /// Default case if none of the given reactions is used
    /// </summary>
    /// <param name="reaction">Reaction</param>
    /// <returns>Result</returns>
    protected override string DefaultFunc(IReaction reaction) => null;

    #endregion // DialogReactionElementBase<bool>
}
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

    #endregion // Fields

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

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("Prompt", "Do you want to add link?");

    /// <inheritdoc/>
    public override IReadOnlyList<ReactionData<string>> GetReactions()
    {
        return _reactions ??= [
                                  new ReactionData<string>
                                  {
                                      Emote = DiscordEmoteService.GetCheckEmote(CommandContext.Client),
                                      Func = RunSubElement<CalendarTemplateUriUriDialogElement, string>
                                  },
                                  new ReactionData<string>
                                  {
                                      Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                                      Func = () => Task.FromResult<string>(null)
                                  }
                              ];
    }

    /// <inheritdoc/>
    protected override string DefaultFunc(IReaction reaction) => null;

    #endregion // DialogReactionElementBase<bool>
}
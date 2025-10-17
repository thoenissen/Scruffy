using Discord;

using Scruffy.Services.Calendar.DialogElements.Forms;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar.DialogElements;

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

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("Prompt", "Do you want to add guild points to this event?");

    /// <inheritdoc/>
    public override IReadOnlyList<ReactionData<CalenderTemplateGuildData>> GetReactions()
    {
        return _reactions ??= [
                                  new ReactionData<CalenderTemplateGuildData>
                                  {
                                      Emote = DiscordEmoteService.GetCheckEmote(CommandContext.Client),
                                      Func = RunSubForm<CalenderTemplateGuildData>,
                                  },
                                  new ReactionData<CalenderTemplateGuildData>
                                  {
                                      Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                                      Func = () => Task.FromResult<CalenderTemplateGuildData>(null)
                                  }
                              ];
    }

    /// <inheritdoc/>
    protected override CalenderTemplateGuildData DefaultFunc(IReaction reaction) => null;

    #endregion // DialogReactionElementBase<bool>
}
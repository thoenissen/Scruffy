using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Add a role preference?
/// </summary>
public class RaidRoleSelectionNextDialogElement : DialogButtonElementBase<bool>
{
    #region Fields

    /// <summary>
    /// First call?
    /// </summary>
    private readonly bool _first;

    /// <summary>
    /// Buttons
    /// </summary>
    private List<ButtonData<bool>> _buttons;

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

    /// <inheritdoc/>
    public override string GetMessage() => _first ? LocalizationGroup.GetText("MessageFirst", "Do you want to add role preference?")
                                               : LocalizationGroup.GetText("MessageNext", "Do you want to add another role preference?");

    /// <inheritdoc/>
    public override IReadOnlyList<ButtonData<bool>> GetButtons()
    {
        return _buttons ??= [
                                new ButtonData<bool>
                                {
                                    CommandText = LocalizationGroup.GetText("Yes", "Yes"),
                                    Emote = DiscordEmoteService.GetCheckEmote(CommandContext.Client),
                                    Func = () => Task.FromResult(true)
                                },
                                new ButtonData<bool>
                                {
                                    CommandText = LocalizationGroup.GetText("No", "No"),
                                    Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                                    Func = () => Task.FromResult(false)
                                },
                            ];
    }

    /// <inheritdoc/>
    protected override bool DefaultFunc() => false;

    #endregion // DialogReactionElementBase<bool>
}